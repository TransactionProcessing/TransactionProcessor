﻿namespace TransactionProcessor.BusinessLogic.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using Manager;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Models;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Services;
    using Settlement.DomainEvents;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.General;
    using Shared.Logger;
    using Transaction.DomainEvents;
    using TransactionAggregate;
    using CalculationType = Models.CalculationType;
    using FeeType = Models.FeeType;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.EventStore.EventHandling.IDomainEventHandler" />
    /// <seealso cref="IDomainEventHandler" />
    public class TransactionDomainEventHandler : IDomainEventHandler
    {
        #region Fields

        /// <summary>
        /// The estate client
        /// </summary>
        private readonly IEstateClient EstateClient;

        /// <summary>
        /// The fee calculation manager
        /// </summary>
        private readonly IFeeCalculationManager FeeCalculationManager;

        /// <summary>
        /// The messaging service client
        /// </summary>
        private readonly IMessagingServiceClient MessagingServiceClient;

        /// <summary>
        /// The security service client
        /// </summary>
        private readonly ISecurityServiceClient SecurityServiceClient;

        private readonly IAggregateRepository<SettlementAggregate, DomainEvent> SettlementAggregateRepository;

        private readonly IAggregateRepository<TransactionAggregate, DomainEvent> TransactionAggregateRepository;

        /// <summary>
        /// The token response
        /// </summary>
        private TokenResponse TokenResponse;

        /// <summary>
        /// The transaction receipt builder
        /// </summary>
        private readonly ITransactionReceiptBuilder TransactionReceiptBuilder;

        #endregion

        #region Constructors

        public TransactionDomainEventHandler(IAggregateRepository<TransactionAggregate, DomainEvent> transactionAggregateRepository,
                                             IFeeCalculationManager feeCalculationManager,
                                             IEstateClient estateClient,
                                             ISecurityServiceClient securityServiceClient,
                                             ITransactionReceiptBuilder transactionReceiptBuilder,
                                             IMessagingServiceClient messagingServiceClient,
                                             IAggregateRepository<SettlementAggregate, DomainEvent> settlementAggregateRepository) {
            this.TransactionAggregateRepository = transactionAggregateRepository;
            this.FeeCalculationManager = feeCalculationManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
            this.TransactionReceiptBuilder = transactionReceiptBuilder;
            this.MessagingServiceClient = messagingServiceClient;
            this.SettlementAggregateRepository = settlementAggregateRepository;
        }

        #endregion

        #region Methods

        public async Task Handle(IDomainEvent domainEvent,
                                 CancellationToken cancellationToken) {
            await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
        }

        private DateTime CalculateSettlementDate(SettlementSchedule merchantSettlementSchedule,
                                                 DateTime completeDateTime) {
            if (merchantSettlementSchedule == SettlementSchedule.Weekly) {
                return completeDateTime.Date.AddDays(7).Date;
            }

            if (merchantSettlementSchedule == SettlementSchedule.Monthly) {
                return completeDateTime.Date.AddMonths(1).Date;
            }

            return completeDateTime.Date;
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        private async Task<TokenResponse> GetToken(CancellationToken cancellationToken) {
            // Get a token to talk to the estate service
            String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
            String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
            Logger.LogInformation($"Client Id is {clientId}");
            Logger.LogInformation($"Client Secret is {clientSecret}");

            if (this.TokenResponse == null) {
                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                return token;
            }

            if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2)) {
                Logger.LogInformation($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                return token;
            }

            return this.TokenResponse;
        }

        private Boolean RequireFeeCalculation(TransactionAggregate transactionAggregate){
            return transactionAggregate switch{
                _ when transactionAggregate.IsAuthorised == false => false,
                _ when transactionAggregate.IsCompleted == false => false,
                _ when transactionAggregate.TransactionType == TransactionType.Logon => false,
                _ when transactionAggregate.ContractId == Guid.Empty => false,
                _ when transactionAggregate.ProductId == Guid.Empty => false,
                _ when transactionAggregate.TransactionAmount == null => false,
                _ => true
            };
        }

        private async Task HandleSpecificDomainEvent(TransactionHasBeenCompletedEvent domainEvent,
                                                     CancellationToken cancellationToken){

            TransactionAggregate transactionAggregate =
                await this.TransactionAggregateRepository.GetLatestVersion(domainEvent.TransactionId, cancellationToken);

            if (RequireFeeCalculation(transactionAggregate) == false)
                return;

            this.TokenResponse = await this.GetToken(cancellationToken);

            List<TransactionFeeToCalculate> feesForCalculation = await this.GetTransactionFeesForCalculation(transactionAggregate, cancellationToken);

            // Do the fee calculation
            List<CalculatedFee> resultFees =
                this.FeeCalculationManager.CalculateFees(feesForCalculation, transactionAggregate.TransactionAmount.Value, domainEvent.CompletedDateTime);

            // Process the non merchant fees
            IEnumerable<CalculatedFee> nonMerchantFees = resultFees.Where(f => f.FeeType == FeeType.ServiceProvider);

            foreach (CalculatedFee calculatedFee in nonMerchantFees){
                // Add Fee to the Transaction 
                transactionAggregate.AddFee(calculatedFee);
            }

            // Now deal with merchant fees 
            List<CalculatedFee> merchantFees = resultFees.Where(f => f.FeeType == FeeType.Merchant).ToList();

            if (merchantFees.Any()){
                MerchantResponse merchant =
                    await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);

                if (merchant.SettlementSchedule == SettlementSchedule.NotSet){
                    throw new NotSupportedException($"Merchant {merchant.MerchantId} does not have a settlement schedule configured");
                }

                foreach (CalculatedFee calculatedFee in merchantFees){
                    // Determine when the fee should be applied
                    DateTime settlementDate = this.CalculateSettlementDate(merchant.SettlementSchedule, domainEvent.CompletedDateTime);

                    transactionAggregate.AddFeePendingSettlement(calculatedFee, settlementDate);


                    if (merchant.SettlementSchedule == SettlementSchedule.Immediate){
                        // Add fees to transaction now if settlement is immediate
                        transactionAggregate.AddSettledFee(calculatedFee,
                                                           settlementDate);
                    }
                }
            }

            await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
        }

        private async Task HandleSpecificDomainEvent(MerchantFeePendingSettlementAddedToTransactionEvent domainEvent,
                                                     CancellationToken cancellationToken){

            Guid aggregateId = Helpers.CalculateSettlementAggregateId(domainEvent.SettlementDueDate.Date, domainEvent.MerchantId, domainEvent.EstateId);

            // We need to add the fees to a pending settlement stream
            SettlementAggregate aggregate = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

            if (aggregate.IsCreated == false) {
                aggregate.Create(domainEvent.EstateId, domainEvent.MerchantId, domainEvent.SettlementDueDate.Date);
            }

            // Create Calculated Fee from the domain event
            CalculatedFee calculatedFee = new CalculatedFee{
                                                               CalculatedValue = domainEvent.CalculatedValue,
                                                               FeeCalculatedDateTime = domainEvent.FeeCalculatedDateTime,
                                                               FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType,
                                                               FeeId = domainEvent.FeeId,
                                                               FeeType = FeeType.Merchant,
                                                               FeeValue = domainEvent.FeeValue,
                                                               SettlementDueDate = domainEvent.SettlementDueDate
                                                           };

            aggregate.AddFee(domainEvent.MerchantId, domainEvent.TransactionId, calculatedFee);

            await this.SettlementAggregateRepository.SaveChanges(aggregate, cancellationToken);
        }

        private async Task HandleSpecificDomainEvent(SettledMerchantFeeAddedToTransactionEvent domainEvent,
                                                     CancellationToken cancellationToken){
            Guid aggregateId = Helpers.CalculateSettlementAggregateId(domainEvent.SettledDateTime.Date, domainEvent.MerchantId, domainEvent.EstateId);

            // We need to add the fees to a pending settlement stream
            SettlementAggregate aggregate = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

            aggregate.MarkFeeAsSettled(domainEvent.MerchantId, domainEvent.TransactionId, domainEvent.FeeId, domainEvent.SettledDateTime.Date);

            await this.SettlementAggregateRepository.SaveChanges(aggregate, cancellationToken);
        }

        private async Task HandleSpecificDomainEvent(MerchantFeeSettledEvent domainEvent,
                                                     CancellationToken cancellationToken)
        {
            TransactionAggregate aggregate = await this.TransactionAggregateRepository.GetLatestVersion(domainEvent.TransactionId, cancellationToken);

            CalculatedFee calculatedFee = new CalculatedFee
                                          {
                                              CalculatedValue = domainEvent.CalculatedValue,
                                              FeeCalculatedDateTime = domainEvent.FeeCalculatedDateTime,
                                              FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType,
                                              FeeId = domainEvent.FeeId,
                                              FeeType = FeeType.Merchant,
                                              FeeValue = domainEvent.FeeValue,
                                              IsSettled = true
                                          };

            aggregate.AddSettledFee(calculatedFee, domainEvent.SettledDateTime);

            await this.TransactionAggregateRepository.SaveChanges(aggregate, cancellationToken);
        }

        private async Task<List<TransactionFeeToCalculate>> GetTransactionFeesForCalculation(TransactionAggregate transactionAggregate, CancellationToken cancellationToken)
        {
            // Ok we should have filtered out the not applicable transactions
            // Get the fees to be calculated
            List<ContractProductTransactionFee> feesForProduct = await this.EstateClient.GetTransactionFeesForProduct(this.TokenResponse.AccessToken,
                                                                                                                      transactionAggregate.EstateId,
                                                                                                                      transactionAggregate.MerchantId,
                                                                                                                      transactionAggregate.ContractId,
                                                                                                                      transactionAggregate.ProductId,
                                                                                                                      cancellationToken);
            List<TransactionFeeToCalculate> feesForCalculation = new List<TransactionFeeToCalculate>();

            foreach (ContractProductTransactionFee contractProductTransactionFee in feesForProduct)
            {
                TransactionFeeToCalculate transactionFeeToCalculate = new TransactionFeeToCalculate
                                                                      {
                                                                          FeeId = contractProductTransactionFee.TransactionFeeId,
                                                                          Value = contractProductTransactionFee.Value,
                                                                          FeeType = (FeeType)contractProductTransactionFee.FeeType,
                                                                          CalculationType =
                                                                              (CalculationType)contractProductTransactionFee.CalculationType
                                                                      };

                feesForCalculation.Add(transactionFeeToCalculate);
            }

            return feesForCalculation;
        }
        
        private async Task HandleSpecificDomainEvent(CustomerEmailReceiptRequestedEvent domainEvent,
                                                     CancellationToken cancellationToken) {
            this.TokenResponse = await this.GetToken(cancellationToken);

            TransactionAggregate transactionAggregate =
                await this.TransactionAggregateRepository.GetLatestVersion(domainEvent.TransactionId, cancellationToken);

            MerchantResponse merchant =
                await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);

            // Determine the body of the email
            String receiptMessage = await this.TransactionReceiptBuilder.GetEmailReceiptMessage(transactionAggregate.GetTransaction(), merchant, cancellationToken);

            // Send the message
            await this.SendEmailMessage(this.TokenResponse.AccessToken,
                                        domainEvent.EventId,
                                        domainEvent.EstateId,
                                        "Transaction Successful",
                                        receiptMessage,
                                        domainEvent.CustomerEmailAddress,
                                        cancellationToken);
        }

        private async Task HandleSpecificDomainEvent(CustomerEmailReceiptResendRequestedEvent domainEvent,
                                                     CancellationToken cancellationToken) {
            this.TokenResponse = await this.GetToken(cancellationToken);

            // Send the message
            await this.ResendEmailMessage(this.TokenResponse.AccessToken, domainEvent.EventId, domainEvent.EstateId, cancellationToken);
        }

        //private async Task HandleSpecificDomainEvent(SettledMerchantFeeAddedToTransactionEvent domainEvent,
        //                                             CancellationToken cancellationToken){
        //    throw new NotImplementedException();
        //    //if (domainEvent.SettlementDueDate == DateTime.MinValue) {
        //    //    // Old event format before settlement
        //    //    return;
        //    //}

        //    Guid aggregateId = Helpers.CalculateSettlementAggregateId(domainEvent.SettlementDueDate, domainEvent.MerchantId, domainEvent.EstateId);

        //    SettlementAggregate pendingSettlementAggregate = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

        //    //pendingSettlementAggregate.MarkFeeAsSettled(domainEvent.MerchantId, domainEvent.TransactionId, domainEvent.FeeId);

        //    //await this.SettlementAggregateRepository.SaveChanges(pendingSettlementAggregate, cancellationToken);
        //}

        private async Task ResendEmailMessage(String accessToken,
                                              Guid messageId,
                                              Guid estateId,
                                              CancellationToken cancellationToken) {
            ResendEmailRequest resendEmailRequest = new ResendEmailRequest {
                                                                               ConnectionIdentifier = estateId,
                                                                               MessageId = messageId
                                                                           };
            try {
                await this.MessagingServiceClient.ResendEmail(accessToken, resendEmailRequest, cancellationToken);
            }
            catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(InvalidOperationException)) {
                // Only bubble up if not a duplicate message
                if (ex.InnerException.Message.Contains("Cannot send a message to provider that has already been sent", StringComparison.InvariantCultureIgnoreCase) ==
                    false) {
                    throw;
                }
            }
        }

        private async Task SendEmailMessage(String accessToken,
                                            Guid messageId,
                                            Guid estateId,
                                            String subject,
                                            String body,
                                            String emailAddress,
                                            CancellationToken cancellationToken) {
            SendEmailRequest sendEmailRequest = new SendEmailRequest {
                                                                         MessageId = messageId,
                                                                         Body = body,
                                                                         ConnectionIdentifier = estateId,
                                                                         FromAddress = "golfhandicapping@btinternet.com", // TODO: lookup from config
                                                                         IsHtml = true,
                                                                         Subject = subject,
                                                                         ToAddresses = new List<String> {
                                                                                                            emailAddress
                                                                                                        }
                                                                     };

            // TODO: may decide to record the message Id againsts the Transaction Aggregate in future, but for now
            // we wont do this...
            try {
                await this.MessagingServiceClient.SendEmail(accessToken, sendEmailRequest, cancellationToken);
            }
            catch(Exception ex) when(ex.InnerException != null && ex.InnerException.GetType() == typeof(InvalidOperationException)) {
                // Only bubble up if not a duplicate message
                if (ex.InnerException.Message.Contains("Cannot send a message to provider that has already been sent", StringComparison.InvariantCultureIgnoreCase) ==
                    false) {
                    throw;
                }
            }
        }

        #endregion
    }
}