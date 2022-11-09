namespace TransactionProcessor.ProjectionEngine.Repository;

using Database;
using Dispatchers;
using Microsoft.EntityFrameworkCore;
using Models;

public class TransactionProcessorReadRepository : ITransactionProcessorReadRepository
{
    private readonly Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> ContextFactory;

    public TransactionProcessorReadRepository(Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> contextFactory) {
        this.ContextFactory = contextFactory;
    }
    public async Task AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry,
                                                     CancellationToken cancellationToken) {
        await using TransactionProcessorGenericContext context = await this.ContextFactory.GetContext(entry.EstateId, cancellationToken);

        TransactionProcessor.ProjectionEngine.Database.Entities.MerchantBalanceChangedEntry entity = new() {
                                                                                                               ChangeAmount = entry.ChangeAmount,
                                                                                                               Reference = entry.Reference,
                                                                                                               AggregateId = entry.AggregateId,
                                                                                                               OriginalEventId = entry.OriginalEventId,
                                                                                                               DebitOrCredit = entry.DebitOrCredit,
                                                                                                               CauseOfChangeId = entry.CauseOfChangeId,
                                                                                                               DateTime = entry.DateTime,
                                                                                                               EstateId = entry.EstateId,
                                                                                                               MerchantId = @entry.MerchantId
                                                                                                           };
        
        await context.MerchantBalanceChangedEntry.AddAsync(entity, cancellationToken);
        
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // We have detected a duplicate, so lets try and update it
            context.Entry(entity).State = EntityState.Modified;

            await context.SaveChangesAsync(cancellationToken);
        }

    }
}