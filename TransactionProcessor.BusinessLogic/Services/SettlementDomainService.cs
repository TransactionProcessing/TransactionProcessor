namespace TransactionProcessor.BusinessLogic.Services
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
    using Models;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Shared.Logger;
    using TransactionAggregate;

    public class SettlementDomainService : ISettlementDomainService
    {
        private readonly IAggregateRepository<TransactionAggregate, DomainEvent> TransactionAggregateRepository;
        private readonly IAggregateRepository<SettlementAggregate, DomainEvent> SettlementAggregateRepository;

        private readonly ISecurityServiceClient SecurityServiceClient;

        private readonly IEstateClient EstateClient;

        public async Task<ProcessSettlementResponse> ProcessSettlement(DateTime settlementDate,
                                                                       Guid estateId,
                                                                       Guid merchantId, 
                                                                       CancellationToken cancellationToken)
        {
            ProcessSettlementResponse response = new ProcessSettlementResponse();

            Guid aggregateId = Helpers.CalculateSettlementAggregateId(settlementDate, merchantId,estateId);

            SettlementAggregate settlementAggregate = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

            if (settlementAggregate.IsCreated == false)
            {
                Logger.LogInformation($"No pending settlement for {settlementDate.ToString("yyyy-MM-dd")}");
                // Not pending settlement for this date
                return response;
            }

            this.TokenResponse = await this.GetToken(cancellationToken);

            MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken,
                                                                            estateId,
                                                                            merchantId,
                                                                            cancellationToken);

            if (merchant.SettlementSchedule == SettlementSchedule.Immediate){
                // Mark the settlement as completed
                settlementAggregate.StartProcessing(DateTime.Now);
                settlementAggregate.ManuallyComplete();
                await this.SettlementAggregateRepository.SaveChanges(settlementAggregate, cancellationToken);
                return response;
            }

            List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> feesToBeSettled = settlementAggregate.GetFeesToBeSettled();
            response.NumberOfFeesPendingSettlement = feesToBeSettled.Count;

            if (feesToBeSettled.Any()){
                // Record the process call
                settlementAggregate.StartProcessing(DateTime.Now);
                await this.SettlementAggregateRepository.SaveChanges(settlementAggregate, cancellationToken);
            }


            foreach ((Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) feeToSettle in feesToBeSettled)
            {
                try
                {
                    TransactionAggregate transactionAggregate = await this.TransactionAggregateRepository.GetLatestVersion(feeToSettle.transactionId, cancellationToken);
                    transactionAggregate.AddSettledFee(feeToSettle.calculatedFee,
                                                       settlementDate,
                                                       DateTime.Now);
                    await this.TransactionAggregateRepository.SaveChanges(transactionAggregate, cancellationToken);
                    response.NumberOfFeesSuccessfullySettled++;
                    response.NumberOfFeesPendingSettlement--;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    response.NumberOfFeesPendingSettlement--;
                    response.NumberOfFeesFailedToSettle++;
                }

            }

            return response;
        }

        public SettlementDomainService(IAggregateRepository<TransactionAggregate, DomainEvent> transactionAggregateRepository,
                                       IAggregateRepository<SettlementAggregate, DomainEvent> settlementAggregateRepository,
                                       ISecurityServiceClient securityServiceClient,
                                       IEstateClient estateClient)
        {
            this.TransactionAggregateRepository = transactionAggregateRepository;
            this.SettlementAggregateRepository = settlementAggregateRepository;
            this.SecurityServiceClient = securityServiceClient;
            this.EstateClient = estateClient;
        }

        private TokenResponse TokenResponse;

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
    }
}