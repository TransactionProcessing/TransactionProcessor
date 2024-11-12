using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    public interface ISettlementDomainService
    {
        Task<Result<Guid>> ProcessSettlement(SettlementCommands.ProcessSettlementCommand command, CancellationToken cancellationToken);

        Task<Result> AddMerchantFeePendingSettlement(SettlementCommands.AddMerchantFeePendingSettlementCommand command,
                                                     CancellationToken cancellationToken);

        Task<Result> AddSettledFeeToSettlement(SettlementCommands.AddSettledFeeToSettlementCommand command,
                                               CancellationToken cancellationToken);
    }
}