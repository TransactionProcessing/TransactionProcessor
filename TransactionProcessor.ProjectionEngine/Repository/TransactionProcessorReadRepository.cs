using Shared.Exceptions;
using Shared.Logger;
using SimpleResults;
using TransactionProcessor.Database.Contexts;

namespace TransactionProcessor.ProjectionEngine.Repository;

using System.Diagnostics.CodeAnalysis;
using Database.Database;
using Microsoft.EntityFrameworkCore;
using Models;
using TransactionProcessor.ProjectionEngine.Database.Database.ViewEntities;

[ExcludeFromCodeCoverage]
public class TransactionProcessorReadRepository : ITransactionProcessorReadRepository
{
    private readonly Shared.EntityFramework.IDbContextFactory<EstateManagementContext> ContextFactory;

    private const String ConnectionStringIdentifier = "EstateReportingReadModel";
    public TransactionProcessorReadRepository(Shared.EntityFramework.IDbContextFactory<EstateManagementContext> contextFactory) {
        this.ContextFactory = contextFactory;
    }
    public async Task<Result> AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry,
                                                             CancellationToken cancellationToken) {

        Logger.LogInformation($"About to add entry {entry.Reference}");
        await using EstateManagementContext context = await this.ContextFactory.GetContext(entry.EstateId, TransactionProcessorReadRepository.ConnectionStringIdentifier, cancellationToken);

        ProjectionEngine.Database.Database.Entities.MerchantBalanceChangedEntry entity = new() {
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

        try {
            await context.SaveChangesAsync(cancellationToken);

            Logger.LogInformation($"Entry added {entry.Reference} new entry");
        }
        catch (DbUpdateException) {
            // We have detected a duplicate, so lets try and update it
            context.Entry(entity).State = EntityState.Modified;
            
            await context.SaveChangesAsync(cancellationToken);

            Logger.LogInformation($"Entry added {entry.Reference} update");
        }
        catch (Exception ex) {
            Logger.LogInformation($"Entry failed {entry.Reference}");
            return Result.Failure(ex.GetExceptionMessages());
        }
        return Result.Success();
    }

    public async Task<Result<List<MerchantBalanceChangedEntry>>> GetMerchantBalanceHistory(Guid estateId,
                                                                                   Guid merchantId,
                                                                                   DateTime startDate,
                                                                                   DateTime endDate,
                                                                                   CancellationToken cancellationToken) {
        await using EstateManagementContext context = await this.ContextFactory.GetContext(estateId, TransactionProcessorReadRepository.ConnectionStringIdentifier, cancellationToken);

        List<MerchantBalanceHistoryViewEntry> entries = await context.MerchantBalanceHistoryViewEntry
                                                                     .Where(v => v.MerchantId == merchantId && v.EntryDateTime >= startDate && v.EntryDateTime <= endDate)
                                                                     .OrderByDescending(e => e.EntryDateTime)
                                                                     .ToListAsync(cancellationToken);

        List<MerchantBalanceChangedEntry> result = new List<MerchantBalanceChangedEntry>();
        entries.ForEach(e => {
                            result.Add(new MerchantBalanceChangedEntry {
                                                                           MerchantId = e.MerchantId,
                                                                           ChangeAmount = e.ChangeAmount,
                                                                           DateTime = e.EntryDateTime,
                                                                           DebitOrCredit = e.DebitOrCredit,
                                                                           OriginalEventId = e.OriginalEventId,
                                                                           Reference = e.Reference,
                                                                       });
                        });
        return Result.Success(result);
    }
}