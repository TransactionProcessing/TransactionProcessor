using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Tests.Mediator;

using System;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Services;
using Models;

public class DummySettlementDomainService : ISettlementDomainService {
    
    public async Task<Result<Guid>> ProcessSettlement(SettlementCommands.ProcessSettlementCommand command,
                                                      CancellationToken cancellationToken) =>
        Result.Success(Guid.NewGuid());

    public async Task<Result> AddMerchantFeePendingSettlement(
        SettlementCommands.AddMerchantFeePendingSettlementCommand command,
        CancellationToken cancellationToken) =>
        Result.Success();

    public async Task<Result> AddSettledFeeToSettlement(SettlementCommands.AddSettledFeeToSettlementCommand command,
                                                        CancellationToken cancellationToken) =>
        Result.Success();
}