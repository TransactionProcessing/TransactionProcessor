namespace TransactionProcessor.ProjectionEngine.Repository;

using System.Diagnostics.CodeAnalysis;
using Models;

public interface ITransactionProcessorReadRepository
{
    Task AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry, CancellationToken cancellationToken);

    Task<List<MerchantBalanceChangedEntry>> GetMerchantBalanceHistory(Guid estateId,
                                                                     Guid merchantId,
                                                                     DateTime startDate,
                                                                     DateTime endDate,
                                                                     CancellationToken cancellationToken);
}