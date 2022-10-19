namespace TransactionProcessor.ProjectionEngine.Database;

using System.Linq.Expressions;
using Entities;
using Microsoft.EntityFrameworkCore;

public abstract class TransactionProcessorGenericContext : DbContext
{
    #region Fields

    protected readonly String ConnectionString;

    protected readonly String DatabaseEngine;

    protected static List<String> TablesToIgnoreDuplicates = new List<String>();

    #endregion

    #region Constructors

    protected TransactionProcessorGenericContext(String databaseEngine,
                                                 String connectionString)
    {
        this.DatabaseEngine = databaseEngine;
        this.ConnectionString = connectionString;
    }

    public TransactionProcessorGenericContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    #endregion

    #region Properties

        

    #endregion

        

    public static Boolean IsDuplicateInsertsIgnored(String tableName) =>
        TransactionProcessorGenericContext.TablesToIgnoreDuplicates.Contains(tableName.Trim(), StringComparer.InvariantCultureIgnoreCase);

    public virtual async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if (this.Database.IsSqlServer() || this.Database.IsMySql())
        {
            await this.Database.MigrateAsync(cancellationToken);
        }
    }

    protected virtual async Task SetIgnoreDuplicates(CancellationToken cancellationToken) {
        TransactionProcessorGenericContext.TablesToIgnoreDuplicates = new List<String> {
                                                                                       };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<MerchantBalanceProjectionState>().HasKey(c => new {
                                                                                  c.EstateId,
                                                                                  c.MerchantId
                                                                              });

        modelBuilder.Entity<MerchantBalanceChangedEntry>().HasKey(c => new {
                                                                               c.AggregateId,
                                                                               c.OriginalEventId
                                                                           });

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<MerchantBalanceProjectionState> MerchantBalanceProjectionState { get; set; }
    public DbSet<MerchantBalanceChangedEntry> MerchantBalanceChangedEntry { get; set; }


}