namespace TransactionProcessor.ProjectionEngine.Repository;

using Database;
using Dispatchers;
using Models;
using Shared.EntityFramework;

public class TransactionProcessorReadRepository : ITransactionProcessorReadRepository
{
    private readonly IDbContextFactory<TransactionProcessorGenericContext> ContextFactory;

    public TransactionProcessorReadRepository(Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> contextFactory) {
        this.ContextFactory = contextFactory;
    }
    public async Task AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry,
                                                     CancellationToken cancellationToken) {
        await using TransactionProcessorGenericContext context = await this.ContextFactory.GetContext(entry.EstateId, cancellationToken);

        TransactionProcessor.ProjectionEngine.Database.Entities.MerchantBalanceChangedEntry entity = new() {
                                                                                                               Balance = entry.Balance,
                                                                                                               ChangeAmount = entry.ChangeAmount,
                                                                                                               Reference = entry.Reference,
                                                                                                               AggregateId = entry.AggregateId,
                                                                                                               OriginalEventId = entry.OriginalEventId,
                                                                                                               DebitOrCredit = entry.DebitOrCredit,
                                                                                                               CauseOfChangeId = entry.CauseOfChangeId,
                                                                                                               DateTime = entry.DateTime,
                                                                                                               EstateId = entry.EstateId,
                                                                                                               MerchantId = @entry.MerchantId,
                                                                                                           };

        await context.MerchantBalanceChangedEntry.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}