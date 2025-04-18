using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.Models.Settlement;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using System.Threading;
    using MediatR;
    using Requests;
    using Services;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;

    public class SettlementRequestHandler : IRequestHandler<SettlementCommands.ProcessSettlementCommand, Result<Guid>>,
                                            IRequestHandler<SettlementCommands.AddMerchantFeePendingSettlementCommand, Result>,
                                            IRequestHandler<SettlementCommands.AddSettledFeeToSettlementCommand, Result>,
                                            IRequestHandler<SettlementQueries.GetPendingSettlementQuery, Result<SettlementAggregate>>,
                                            IRequestHandler<SettlementQueries.GetSettlementQuery, Result<SettlementModel>>,
                                            IRequestHandler<SettlementQueries.GetSettlementsQuery, Result<List<SettlementModel>>>
    {
        private readonly ISettlementDomainService SettlementDomainService;
        private readonly IAggregateService AggregateService;
        private readonly ITransactionProcessorManager TransactionProcessorManager;

        public SettlementRequestHandler(ISettlementDomainService settlementDomainService,
                                        IAggregateService aggregateService,
                                        ITransactionProcessorManager transactionProcessorManager) {
            this.SettlementDomainService = settlementDomainService;
            this.AggregateService = aggregateService;
            this.TransactionProcessorManager = transactionProcessorManager;
        }

        public async Task<Result<Guid>> Handle(SettlementCommands.ProcessSettlementCommand command,
                                                            CancellationToken cancellationToken)
        {
            return await this.SettlementDomainService.ProcessSettlement(command, cancellationToken);
        }

        public async Task<Result> Handle(SettlementCommands.AddMerchantFeePendingSettlementCommand command,
                                         CancellationToken cancellationToken) {
            return await this.SettlementDomainService.AddMerchantFeePendingSettlement(command, cancellationToken);
        }

        public async Task<Result> Handle(SettlementCommands.AddSettledFeeToSettlementCommand command,
                                         CancellationToken cancellationToken) {
            return await this.SettlementDomainService.AddSettledFeeToSettlement(command, cancellationToken);
        }

        public async Task<Result<SettlementAggregate>> Handle(SettlementQueries.GetPendingSettlementQuery query,
                                                              CancellationToken cancellationToken) {
            // TODO: Convert to using a manager/model/factory
            // Convert the date passed in to a guid
            Guid aggregateId = Helpers.CalculateSettlementAggregateId(query.SettlementDate, query.MerchantId, query.EstateId);
            
            Result<SettlementAggregate> getSettlementResult = await this.AggregateService.GetLatest<SettlementAggregate>(aggregateId, cancellationToken);
            if (getSettlementResult.IsFailed)
                return getSettlementResult;

            SettlementAggregate settlementAggregate = getSettlementResult.Data;

            return Result.Success(settlementAggregate);
        }

        public async Task<Result<SettlementModel>> Handle(SettlementQueries.GetSettlementQuery request,
                                                          CancellationToken cancellationToken) {
            return await this.TransactionProcessorManager.GetSettlement(request.EstateId, request.MerchantId, request.SettlementId, cancellationToken);
        }

        public async Task<Result<List<SettlementModel>>> Handle(SettlementQueries.GetSettlementsQuery request,
                                                                CancellationToken cancellationToken) {
            return await this.TransactionProcessorManager.GetSettlements(request.EstateId, request.MerchantId, request.StartDate, request.EndDate, cancellationToken);
        }
    }
}
