namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using Models;

public class DummySettlementDomainService : ISettlementDomainService
{
    public async Task<ProcessSettlementResponse> ProcessSettlement(DateTime pendingSettlementDate,
                                                                   Guid estateId,
                                                                   Guid merchantId,
                                                                   CancellationToken cancellationToken) {
        return new ProcessSettlementResponse();
    }
}