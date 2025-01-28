using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Common;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using System.Threading;
    using MediatR;
    using Models;
    using Requests;
    using Services;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    public class SettlementRequestHandler : IRequestHandler<SettlementCommands.ProcessSettlementCommand, Result<Guid>>,
                                            IRequestHandler<SettlementCommands.AddMerchantFeePendingSettlementCommand, Result>,
                                            IRequestHandler<SettlementCommands.AddSettledFeeToSettlementCommand, Result>,
                                            IRequestHandler<SettlementQueries.GetPendingSettlementQuery, Result<SettlementAggregate>> {
        private readonly ISettlementDomainService SettlementDomainService;
        private readonly IAggregateRepository<SettlementAggregate, DomainEvent> SettlementAggregateRepository;

        public SettlementRequestHandler(ISettlementDomainService settlementDomainService,
                                        IAggregateRepository<SettlementAggregate, DomainEvent> settlementAggregateRepository) {
            this.SettlementDomainService = settlementDomainService;
            this.SettlementAggregateRepository = settlementAggregateRepository;
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
            
            Result<SettlementAggregate> getSettlementResult = await this.SettlementAggregateRepository.GetLatestVersion(aggregateId, cancellationToken);
            if (getSettlementResult.IsFailed)
                return getSettlementResult;

            SettlementAggregate settlementAggregate = getSettlementResult.Data;

            return Result.Success(settlementAggregate);
        }
    }
}
