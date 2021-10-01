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
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses;
    using Manager;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Models;
    using Newtonsoft.Json;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Services;
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
        /// The security service client
        /// </summary>
        private readonly ISecurityServiceClient SecurityServiceClient;

        /// <summary>
        /// The transaction receipt builder
        /// </summary>
        private readonly ITransactionReceiptBuilder TransactionReceiptBuilder;

        /// <summary>
        /// The messaging service client
        /// </summary>
        private readonly IMessagingServiceClient MessagingServiceClient;

        private readonly IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent> SettlementAggregateRepository;

        /// <summary>
        /// The token response
        /// </summary>
        private TokenResponse TokenResponse;

        /// <summary>
        /// The transaction aggregate manager
        /// </summary>
        private readonly ITransactionAggregateManager TransactionAggregateManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDomainEventHandler" /> class.
        /// </summary>
        /// <param name="transactionAggregateManager">The transaction aggregate manager.</param>
        /// <param name="feeCalculationManager">The fee calculation manager.</param>
        /// <param name="estateClient">The estate client.</param>
        /// <param name="securityServiceClient">The security service client.</param>
        /// <param name="transactionReceiptBuilder">The transaction receipt builder.</param>
        /// <param name="messagingServiceClient">The messaging service client.</param>
        public TransactionDomainEventHandler(ITransactionAggregateManager transactionAggregateManager,
                                             IFeeCalculationManager feeCalculationManager,
                                             IEstateClient estateClient,
                                             ISecurityServiceClient securityServiceClient,
                                             ITransactionReceiptBuilder transactionReceiptBuilder,
                                             IMessagingServiceClient messagingServiceClient,
                                             IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent> settlementAggregateRepository)
        {
            this.TransactionAggregateManager = transactionAggregateManager;
            this.FeeCalculationManager = feeCalculationManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
            this.TransactionReceiptBuilder = transactionReceiptBuilder;
            this.MessagingServiceClient = messagingServiceClient;
            this.SettlementAggregateRepository = settlementAggregateRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the specified domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task Handle(IDomainEvent domainEvent,
                                 CancellationToken cancellationToken)
        {
            await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
        }

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        private async Task<TokenResponse> GetToken(CancellationToken cancellationToken)
        {
            // Get a token to talk to the estate service
            String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
            String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
            Logger.LogInformation($"Client Id is {clientId}");
            Logger.LogInformation($"Client Secret is {clientSecret}");

            if (this.TokenResponse == null)
            {
                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                return token;
            }

            if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2))
            {
                Logger.LogInformation($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                return token;
            }

            return this.TokenResponse;
        }

        /// <summary>
        /// Handles the specific domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task HandleSpecificDomainEvent(TransactionHasBeenCompletedEvent domainEvent,
                                                     CancellationToken cancellationToken)
        {
            TransactionAggregate transactionAggregate =
                await this.TransactionAggregateManager.GetAggregate(domainEvent.EstateId, domainEvent.TransactionId, cancellationToken);

            if (transactionAggregate.IsAuthorised == false)
            {
                // Ignore not successful transactions
                return;
            }

            if (transactionAggregate.IsCompleted == false || transactionAggregate.TransactionType == TransactionType.Logon ||
                (transactionAggregate.ContractId == Guid.Empty || transactionAggregate.ProductId == Guid.Empty))
            {
                // These transactions cannot have fee values calculated so skip
                return;
            }

            this.TokenResponse = await this.GetToken(cancellationToken);
            
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
                                                                          CalculationType = (CalculationType)contractProductTransactionFee.CalculationType
                                                                      };

                feesForCalculation.Add(transactionFeeToCalculate);
            }

            // Do the fee calculation
            List<CalculatedFee> resultFees = this.FeeCalculationManager.CalculateFees(feesForCalculation, transactionAggregate.TransactionAmount.Value);

            // Process the non merchant fees
            IEnumerable<CalculatedFee> nonMerchantFees = resultFees.Where(f => f.FeeType == FeeType.ServiceProvider);

            foreach (CalculatedFee calculatedFee in nonMerchantFees)
            {
                // Add Fee to the Transaction 
                await this.TransactionAggregateManager.AddFee(transactionAggregate.EstateId, transactionAggregate.AggregateId, calculatedFee, cancellationToken);
            }

            // Now deal with merchant fees 
            IEnumerable<CalculatedFee> merchantFees = resultFees.Where(f => f.FeeType == FeeType.Merchant);

            // get the merchant now to see the settlement schedule
            this.TokenResponse = await this.GetToken(cancellationToken);
            MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);
                        
            // Add fees to transaction now if settlement is immediate
            if (merchant.SettlementSchedule == SettlementSchedule.Immediate)
            {
                foreach (CalculatedFee calculatedFee in merchantFees)
                {
                    // Add Fee to the Transaction 
                    await this.TransactionAggregateManager.AddSettledFee(transactionAggregate.EstateId, transactionAggregate.AggregateId, calculatedFee, DateTime.Now.Date, DateTime.Now, cancellationToken);
                }
            }
            else if (merchant.SettlementSchedule == SettlementSchedule.NotSet)
            {
                throw new NotSupportedException($"Merchant {merchant.MerchantId} does not have a settlement schedule configured");
            }
            else
            {
                foreach (CalculatedFee calculatedFee in merchantFees)
                {
                    // Determine when the fee should be applied
                    Guid aggregateId = merchant.NextSettlementDueDate.ToGuid();

                    // We need to add the fees to a pending settlement stream (for today)
                    SettlementAggregate aggregate = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

                    if (aggregate.IsCreated == false)
                    {
                        aggregate.Create(transactionAggregate.EstateId, merchant.NextSettlementDueDate);
                    }

                    aggregate.AddFee(transactionAggregate.MerchantId, transactionAggregate.AggregateId, calculatedFee);

                    await this.SettlementAggregateRepository.SaveChanges(aggregate, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handles the specific domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task HandleSpecificDomainEvent(CustomerEmailReceiptRequestedEvent domainEvent,
                                                     CancellationToken cancellationToken)
        {
            this.TokenResponse = await this.GetToken(cancellationToken);

            TransactionAggregate transactionAggregate = await this.TransactionAggregateManager.GetAggregate(domainEvent.EstateId, domainEvent.TransactionId, cancellationToken);

            MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);

            // Determine the body of the email
            String receiptMessage = await this.TransactionReceiptBuilder.GetEmailReceiptMessage(transactionAggregate.GetTransaction(), merchant, cancellationToken);

            // Send the message
            await this.SendEmailMessage(this.TokenResponse.AccessToken, domainEvent.EventId, domainEvent.EstateId, "Transaction Successful", receiptMessage, domainEvent.CustomerEmailAddress, cancellationToken);

        }

        private async Task HandleSpecificDomainEvent(MerchantFeeAddedToTransactionEvent domainEvent,
                                                     CancellationToken cancellationToken)
        {
            if (domainEvent.SettlementDueDate == DateTime.MinValue)
            {
                // Old event format before settlement
                return;
            }

            Guid aggregateId = domainEvent.SettlementDueDate.ToGuid();

            SettlementAggregate pendingSettlementAggregate = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

            pendingSettlementAggregate.MarkFeeAsSettled(domainEvent.MerchantId, domainEvent.TransactionId, domainEvent.FeeId);

            await this.SettlementAggregateRepository.SaveChanges(pendingSettlementAggregate, cancellationToken);
        }

        /// <summary>
        /// Sends the email message.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task SendEmailMessage(String accessToken, 
                                            Guid messageId,
                                            Guid estateId,
                                            String subject,
                                            String body,
                                            String emailAddress,
                                            CancellationToken cancellationToken)
        {
            SendEmailRequest sendEmailRequest = new SendEmailRequest
                                                {
                                                    MessageId = messageId,
                                                    Body = body,
                                                    ConnectionIdentifier = estateId,
                                                    FromAddress = "golfhandicapping@btinternet.com", // TODO: lookup from config
                                                    IsHtml = true,
                                                    Subject = subject,
                                                    ToAddresses = new List<String>
                                                                  {
                                                                      emailAddress
                                                                  }
                                                };

            // TODO: may decide to record the message Id againsts the Transaction Aggregate in future, but for now
            // we wont do this...
            try
            {
                await this.MessagingServiceClient.SendEmail(accessToken, sendEmailRequest, cancellationToken);
            }
            catch(Exception ex) when (ex.InnerException != null && ex.InnerException.GetType() == typeof(InvalidOperationException))
            {
                // Only bubble up if not a duplicate message
                if (ex.InnerException.Message.Contains("Cannot send a message to provider that has already been sent", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    throw;
                }
            }
            
        }

        #endregion
    }
}