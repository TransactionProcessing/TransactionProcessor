using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.RequestHandlers
{
    using System.Threading;
    using MediatR;
    using Models;
    using Requests;
    using Services;

    public class SettlementRequestHandler : IRequestHandler<ProcessSettlementRequest, ProcessSettlementResponse>
    {
        private readonly ISettlementDomainService SettlementDomainService;

        public SettlementRequestHandler(ISettlementDomainService settlementDomainService)
        {
            this.SettlementDomainService = settlementDomainService;
        }

        public async Task<ProcessSettlementResponse> Handle(ProcessSettlementRequest request,
                                                            CancellationToken cancellationToken)
        {
            ProcessSettlementResponse processSettlementResponse = await this.SettlementDomainService.ProcessSettlement(request.SettlementDate, request.EstateId, cancellationToken);

            return processSettlementResponse;
        }
    }
}
