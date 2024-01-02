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

            this.TokenResponse = await Helpers.GetToken(this.TokenResponse, this.SecurityServiceClient, cancellationToken);

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
                                                       settlementAggregate.SettlementDate,
                                                       settlementAggregate.AggregateId);
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
    }
}