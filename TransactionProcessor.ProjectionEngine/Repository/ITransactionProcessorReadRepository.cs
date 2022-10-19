namespace TransactionProcessor.ProjectionEngine.Repository;

using Dispatchers;
using Models;

public interface ITransactionProcessorReadRepository
{
    Task AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry, CancellationToken cancellationToken);
}