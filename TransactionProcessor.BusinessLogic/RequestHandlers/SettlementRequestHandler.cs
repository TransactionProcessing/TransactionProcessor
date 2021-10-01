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
        private readonly ITransactionDomainService TransactionDomainService;

        public SettlementRequestHandler(ITransactionDomainService transactionDomainService)
        {
            this.TransactionDomainService = transactionDomainService;
        }

        public async Task<ProcessSettlementResponse> Handle(ProcessSettlementRequest request,
                                                            CancellationToken cancellationToken)
        {
            ProcessSettlementResponse processSettlementResponse = await this.TransactionDomainService.ProcessSettlement(request.SettlementDate, request.EstateId, cancellationToken);

            return processSettlementResponse;
        }
    }
}
