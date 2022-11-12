namespace TransactionProcessor.ProjectionEngine.Repository;

using Database;
using Database.ViewEntities;
using Dispatchers;
using Microsoft.EntityFrameworkCore;
using Models;
using static Grpc.Core.Metadata;

public class TransactionProcessorReadRepository : ITransactionProcessorReadRepository
{
    private readonly Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> ContextFactory;

    private const String ConnectionStringIdentifier = "TransactionProcessorReadModel";
    public TransactionProcessorReadRepository(Shared.EntityFramework.IDbContextFactory<TransactionProcessorGenericContext> contextFactory) {
        this.ContextFactory = contextFactory;
    }
    public async Task AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry,
                                                     CancellationToken cancellationToken) {
        await using TransactionProcessorGenericContext context = await this.ContextFactory.GetContext(entry.EstateId, ConnectionStringIdentifier, cancellationToken);

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

    public async Task<List<MerchantBalanceChangedEntry>> GetMerchantBalanceHistory(Guid estateId,
                                                                                   Guid merchantId,
                                                                                   DateTime startDate,
                                                                                   DateTime endDate,
                                                                                   CancellationToken cancellationToken) {
        await using TransactionProcessorGenericContext context = await this.ContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

        List<MerchantBalanceHistoryViewEntry> entries = await context.MerchantBalanceHistoryViewEntry
                                                                     .Where(v => v.MerchantId == merchantId && v.EntryDateTime >= startDate && v.EntryDateTime <= endDate)
                                                                     .OrderByDescending(e => e.EntryDateTime)
                                                                     .ToListAsync(cancellationToken);

        List<MerchantBalanceChangedEntry> result = new List<MerchantBalanceChangedEntry>();
        entries.ForEach(e => {
                            result.Add(new MerchantBalanceChangedEntry {
                                                                           Balance = e.Balance,
                                                                           MerchantId = e.MerchantId,
                                                                           ChangeAmount = e.ChangeAmount,
                                                                           DateTime = e.EntryDateTime,
                                                                           DebitOrCredit = e.DebitOrCredit,
                                                                           OriginalEventId = e.OriginalEventId,
                                                                           Reference = e.Reference,
                                                                       });
                        });
        return result;
    }
}