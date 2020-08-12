﻿namespace TransactionProcessor.BusinessLogic.EventHandling
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
        /// Initializes a new instance of the <see cref="TransactionDomainEventHandler"/> class.
        /// </summary>
        /// <param name="transactionAggregateManager">The transaction aggregate manager.</param>
        /// <param name="feeCalculationManager">The fee calculation manager.</param>
        /// <param name="estateClient">The estate client.</param>
        /// <param name="securityServiceClient">The security service client.</param>
        public TransactionDomainEventHandler(ITransactionAggregateManager transactionAggregateManager,
                                             IFeeCalculationManager feeCalculationManager,
                                             IEstateClient estateClient,
                                             ISecurityServiceClient securityServiceClient)
        {
            this.TransactionAggregateManager = transactionAggregateManager;
            this.FeeCalculationManager = feeCalculationManager;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
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

        #endregion
    }
}