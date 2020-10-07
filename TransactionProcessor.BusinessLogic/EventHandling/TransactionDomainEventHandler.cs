namespace TransactionProcessor.BusinessLogic.EventHandling
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Responses;
    using Manager;
    using MessagingService.BusinessLogic.EventHandling;
    using MessagingService.Client;
    using MessagingService.DataTransferObjects;
    using Models;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Services;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.General;
    using Shared.Logger;
    using Transaction.DomainEvents;
    using TransactionAggregate;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MessagingService.BusinessLogic.EventHandling.IDomainEventHandler" />
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

        private readonly ITransactionReceiptBuilder TransactionReceiptBuilder;

        private readonly IMessagingServiceClient MessagingServiceClient;

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
                                             IMessagingServiceClient messagingServiceClient)
        {
            this.TransactionAggregateManager = transactionAggregateManager;
            this.FeeCalculationManager = feeCalculationManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
            this.TransactionReceiptBuilder = transactionReceiptBuilder;
            this.MessagingServiceClient = messagingServiceClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the specified domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task Handle(DomainEvent domainEvent,
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

            foreach (CalculatedFee calculatedFee in resultFees)
            {
                // Add Fee to the Transaction 
                await this.TransactionAggregateManager.AddFee(transactionAggregate.EstateId, transactionAggregate.AggregateId, calculatedFee, cancellationToken);
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

            var merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, domainEvent.EstateId, domainEvent.MerchantId, cancellationToken);

            // Determine the body of the email
            String receiptMessage = await this.TransactionReceiptBuilder.GetEmailReceiptMessage(transactionAggregate.GetTransaction(), merchant, cancellationToken);

            // Send the message
            await this.SendEmailMessage(this.TokenResponse.AccessToken, domainEvent.EstateId, "Transaction Successful", receiptMessage, domainEvent.CustomerEmailAddress, cancellationToken);

        }

        /// <summary>
        /// Sends the email message.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task SendEmailMessage(String accessToken, 
                                            Guid estateId,
                                            String subject,
                                            String body,
                                            String emailAddress,
                                            CancellationToken cancellationToken)
        {
            SendEmailRequest sendEmailRequest = new SendEmailRequest
                                                {
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
            await this.MessagingServiceClient.SendEmail(accessToken, sendEmailRequest, cancellationToken);
        }

        #endregion
    }
}