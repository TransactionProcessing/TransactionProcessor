using TransactionProcessing.UnionResults;

namespace TransactionProcessor.ProjectionEngine.Repository;

using Models;

public interface ITransactionProcessorReadRepository
{
    Task<Result> AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry, CancellationToken cancellationToken);

    Task<Result<List<MerchantBalanceChangedEntry>>> GetMerchantBalanceHistory(Guid estateId,
                                                                     Guid merchantId,
                                                                     DateTimeOffset startDate,
                                                                     DateTimeOffset endDate,
                                                                     CancellationToken cancellationToken);
}