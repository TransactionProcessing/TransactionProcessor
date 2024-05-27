namespace TransactionProcessor.BusinessLogic.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.Database.Entities;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Responses.Estate;
    using FloatAggregate;
    using Manager;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Microsoft.Extensions.Caching.Memory;
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
    using ContractProductTransactionFee = EstateManagement.DataTransferObjects.Responses.Contract.ContractProductTransactionFee;
    using FeeType = Models.FeeType;
    using MerchantResponse = EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse;
    using Transaction = Models.Transaction;

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

        private readonly IAggregateRepository<FloatAggregate, DomainEvent> FloatAggregateRepository;

        private readonly IMemoryCacheWrapper MemoryCache;

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
                                             IAggregateRepository<SettlementAggregate, DomainEvent> settlementAggregateRepository,
                                             IAggregateRepository<FloatAggregate, DomainEvent> floatAggregateRepository,
                                             IMemoryCacheWrapper memoryCache) {
            this.TransactionAggregateRepository = transactionAggregateRepository;
            this.FeeCalculationManager = feeCalculationManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
            this.TransactionReceiptBuilder = transactionReceiptBuilder;
            this.MessagingServiceClient = messagingServiceClient;
            this.SettlementAggregateRepository = settlementAggregateRepository;
            this.FloatAggregateRepository = floatAggregateRepository;
            this.MemoryCache = memoryCache;
        }

        #endregion

        #region Methods

        public async Task Handle(IDomainEvent domainEvent,
                                 CancellationToken cancellationToken) {
            await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
        }

        internal static DateTime CalculateSettlementDate(EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule merchantSettlementSchedule,
                                                         DateTime completeDateTime) {
            if (merchantSettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly) {
                return completeDateTime.Date.AddDays(7).Date;
            }

            if (merchantSettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly) {
                return completeDateTime.Date.AddMonths(1).Date;
            }

            return completeDateTime.Date;
        }
        
        internal static Boolean RequireFeeCalculation(TransactionAggregate transactionAggregate){
            return transactionAggregate switch{
                _ when transactionAggregate.TransactionType == TransactionType.Logon => false,
                _ when transactionAggregate.IsAuthorised == false => false,
                _ when transactionAggregate.IsCompleted == false => false,
                _ when transactionAggregate.ContractId == Guid.Empty => false,
                _ when transactionAggregate.TransactionAmount == null => false,
                _ => true
            };
        }

        private async Task HandleSpecificDomainEvent(TransactionCostInformationRecordedEvent domainEvent, CancellationToken cancellationToken){
            TransactionAggregate transactionAggregate =
                await this.TransactionAggregateRepository.GetLatestVersion(domainEvent.TransactionId, cancellationToken);

            if (transactionAggregate.IsAuthorised == false || transactionAggregate.IsCompleted == false)
                return;

            Guid floatAggregateId = IdGenerationService.GenerateFloatAggregateId(transactionAggregate.EstateId, transactionAggregate.ContractId, transactionAggregate.ProductId);
            FloatAggregate floatAggregate = await this.FloatAggregateRepository.GetLatestVersion(floatAggregateId, cancellationToken);

            floatAggregate.RecordTransactionAgainstFloat(transactionAggregate.AggregateId, transactionAggregate.TransactionAmount.GetValueOrDefault());
            
            await this.FloatAggregateRepository.SaveChanges(floatAggregate, cancellationToken);
        }

        private async Task HandleSpecificDomainEvent(TransactionHasBeenCompletedEvent domainEvent,
                                                     CancellationToken cancellationToken){

            TransactionAggregate transactionAggregate =
                await this.TransactionAggregateRepository.GetLatestVersion(domainEvent.TransactionId, cancellationToken);
            
            if (RequireFeeCalculation(transactionAggregate) == false)
                return;

            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

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
                EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant =
                    await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);

                if (merchant.SettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.NotSet){
                    throw new NotSupportedException($"Merchant {merchant.MerchantId} does not have a settlement schedule configured");
                }

                foreach (CalculatedFee calculatedFee in merchantFees){
                    // Determine when the fee should be applied
                    DateTime settlementDate = TransactionDomainEventHandler.CalculateSettlementDate(merchant.SettlementSchedule, domainEvent.CompletedDateTime);

                    transactionAggregate.AddFeePendingSettlement(calculatedFee, settlementDate);


                    if (merchant.SettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate){
                        Guid settlementId = Helpers.CalculateSettlementAggregateId(settlementDate, domainEvent.MerchantId, domainEvent.EstateId);
                        
                        // Add fees to transaction now if settlement is immediate
                        transactionAggregate.AddSettledFee(calculatedFee,
                                                           settlementDate,
                                                           settlementId);
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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            Guid aggregateId = Helpers.CalculateSettlementAggregateId(domainEvent.SettledDateTime.Date, domainEvent.MerchantId, domainEvent.EstateId);

            EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);

            // We need to add the fees to a pending settlement stream
            SettlementAggregate aggregate = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);


            if (merchant.SettlementSchedule == EstateManagement.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate){
                aggregate.ImmediatelyMarkFeeAsSettled(domainEvent.MerchantId, domainEvent.TransactionId, domainEvent.FeeId);
            }
            else {
                aggregate.MarkFeeAsSettled(domainEvent.MerchantId, domainEvent.TransactionId, domainEvent.FeeId, domainEvent.SettledDateTime.Date);
            }

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

            aggregate.AddSettledFee(calculatedFee, domainEvent.SettledDateTime, domainEvent.SettlementId);

            await this.TransactionAggregateRepository.SaveChanges(aggregate, cancellationToken);
        }

        private async Task<List<TransactionFeeToCalculate>> GetTransactionFeesForCalculation(TransactionAggregate transactionAggregate, CancellationToken cancellationToken){

            // Ok we should have filtered out the not applicable transactions
            // Check if we have fees for this product in the cache
            Boolean feesInCache = this.MemoryCache.TryGetValue((transactionAggregate.EstateId, transactionAggregate.ContractId, transactionAggregate.ProductId),
                                                                                                    out List<ContractProductTransactionFee> feesForProduct);

            if (feesInCache == false){
                Logger.LogInformation($"Fees for Key: Estate Id {transactionAggregate.EstateId} Contract Id {transactionAggregate.ContractId} ProductId {transactionAggregate.ProductId} not found in the cache");

                // Nothing in cache so we need to make a remote call
                // Get the fees to be calculated
                feesForProduct = await this.EstateClient.GetTransactionFeesForProduct(this.TokenResponse.AccessToken,
                                                                                      transactionAggregate.EstateId,
                                                                                      transactionAggregate.MerchantId,
                                                                                      transactionAggregate.ContractId,
                                                                                      transactionAggregate.ProductId,
                                                                                      cancellationToken);
                // Now add this the result to the cache
                String contractProductFeeCacheExpiryInHours = ConfigurationReader.GetValue("ContractProductFeeCacheExpiryInHours");
                if (String.IsNullOrEmpty(contractProductFeeCacheExpiryInHours)){
                    contractProductFeeCacheExpiryInHours = "168"; // 7 Days default
                }
                this.MemoryCache.Set((transactionAggregate.EstateId, transactionAggregate.ContractId, transactionAggregate.ProductId),
                                     feesForProduct,
                                     new MemoryCacheEntryOptions(){
                                                                      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(Int32.Parse(contractProductFeeCacheExpiryInHours))
                                                                  });
                Logger.LogInformation($"Fees for Key: Estate Id {transactionAggregate.EstateId} Contract Id {transactionAggregate.ContractId} ProductId {transactionAggregate.ProductId} added to cache");

            }
            else
            {
                Logger.LogInformation($"Fees for Key: Estate Id {transactionAggregate.EstateId} Contract Id {transactionAggregate.ContractId} ProductId {transactionAggregate.ProductId} found in the cache");
            }
            
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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            TransactionAggregate transactionAggregate =
                await this.TransactionAggregateRepository.GetLatestVersion(domainEvent.TransactionId, cancellationToken);
            Transaction transaction = transactionAggregate.GetTransaction();

            MerchantResponse merchant =
                await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);

            EstateResponse estate = await this.EstateClient.GetEstate(this.TokenResponse.AccessToken, domainEvent.EstateId, cancellationToken);
            EstateOperatorResponse @operator = estate.Operators.Single(o => o.OperatorId == transaction.OperatorId);

            // Determine the body of the email
            String receiptMessage = await this.TransactionReceiptBuilder.GetEmailReceiptMessage(transactionAggregate.GetTransaction(), merchant, @operator.Name, cancellationToken);

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
            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

            // Send the message
            await this.ResendEmailMessage(this.TokenResponse.AccessToken, domainEvent.EventId, domainEvent.EstateId, cancellationToken);
        }
        
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