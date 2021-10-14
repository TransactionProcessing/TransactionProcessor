namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Models;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Logger;

    public class SettlementDomainService : ISettlementDomainService
    {
        private readonly ITransactionAggregateManager TransactionAggregateManager;

        private readonly IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent> SettlementAggregateRepository;

        public async Task<ProcessSettlementResponse> ProcessSettlement(DateTime settlementDate,
                                                                       Guid estateId,
                                                                       CancellationToken cancellationToken)
        {
            ProcessSettlementResponse response = new ProcessSettlementResponse();

            Guid aggregateId = settlementDate.ToGuid();

            SettlementAggregate settlementAggregate = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);

            if (settlementAggregate.IsCreated == false)
            {
                // Not pending settlement for this date
                return response;
            }

            var feesToBeSettled = settlementAggregate.GetFeesToBeSettled();
            response.NumberOfFeesPendingSettlement = feesToBeSettled.Count;

            foreach ((Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) feeToSettle in feesToBeSettled)
            {
                try
                {
                    await this.TransactionAggregateManager.AddSettledFee(estateId,
                                                                         feeToSettle.transactionId,
                                                                         feeToSettle.calculatedFee,
                                                                         settlementDate,
                                                                         DateTime.Now,
                                                                         cancellationToken);
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

        public SettlementDomainService(ITransactionAggregateManager transactionAggregateManager,
                                       IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent> settlementAggregateRepository)
        {
            this.TransactionAggregateManager = transactionAggregateManager;
            this.SettlementAggregateRepository = settlementAggregateRepository;
        }
    }
}