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
                                            IRequestHandler<SettlementQueries.GetPendingSettlementQuery, Result<PendingSettlementModel>>,
                                            IRequestHandler<SettlementQueries.GetSettlementQuery, Result<SettlementModel>>,
                                            IRequestHandler<SettlementQueries.GetSettlementsQuery, Result<List<SettlementModel>>>
    {
        private readonly ISettlementDomainService SettlementDomainService;
        private readonly ITransactionProcessorManager TransactionProcessorManager;

        public SettlementRequestHandler(ISettlementDomainService settlementDomainService,
                                        ITransactionProcessorManager transactionProcessorManager) {
            this.SettlementDomainService = settlementDomainService;
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

        public async Task<Result<PendingSettlementModel>> Handle(SettlementQueries.GetPendingSettlementQuery query,
                                                                 CancellationToken cancellationToken) {
            return await this.TransactionProcessorManager.GetPendingSettlement(query.EstateId, query.MerchantId, query.SettlementDate, cancellationToken);
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
