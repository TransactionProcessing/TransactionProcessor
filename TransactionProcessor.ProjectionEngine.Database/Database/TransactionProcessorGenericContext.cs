namespace TransactionProcessor.ProjectionEngine.Database.Database;

using System.Reflection;
using Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Logger;
using ViewEntities;

public abstract class TransactionProcessorGenericContext : DbContext
{
    #region Fields

    protected readonly String ConnectionString;

    protected readonly String DatabaseEngine;

    protected static List<String> TablesToIgnoreDuplicates = new List<String>();

    #endregion

    #region Constructors

    protected TransactionProcessorGenericContext(String databaseEngine,
                                                 String connectionString) {
        this.DatabaseEngine = databaseEngine;
        this.ConnectionString = connectionString;
    }

    public TransactionProcessorGenericContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {
    }

    #endregion

    #region Properties

    public DbSet<MerchantBalanceChangedEntry> MerchantBalanceChangedEntry { get; set; }

    public DbSet<MerchantBalanceHistoryViewEntry> MerchantBalanceHistoryViewEntry { get; set; }

    public DbSet<MerchantBalanceProjectionState> MerchantBalanceProjectionState { get; set; }
    public DbSet<Event> Events { get; set; }
    #endregion

    #region Methods

    public static Boolean IsDuplicateInsertsIgnored(String tableName) =>
        TransactionProcessorGenericContext.TablesToIgnoreDuplicates.Contains(tableName.Trim(), StringComparer.InvariantCultureIgnoreCase);

    public virtual async Task MigrateAsync(CancellationToken cancellationToken) {
        if (this.Database.IsSqlServer() || this.Database.IsMySql()) {
            await this.Database.MigrateAsync(cancellationToken);
            await this.CreateViews(cancellationToken);
            await this.SetIgnoreDuplicates(cancellationToken);
        }
    }

    protected virtual async Task SetIgnoreDuplicates(CancellationToken cancellationToken) {
        TransactionProcessorGenericContext.TablesToIgnoreDuplicates = new List<String> {
                                                                                           nameof(this.MerchantBalanceProjectionState),
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

        modelBuilder.Entity<MerchantBalanceHistoryViewEntry>().HasNoKey().ToView("uvwMerchantBalanceHistory");

        modelBuilder.Entity<Event>().HasKey(t => new {
                                                         t.EventId,
                                                         t.Type
                                                     }).IsClustered();

        base.OnModelCreating(modelBuilder);
    }

    private async Task CreateViews(CancellationToken cancellationToken) {
        String executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        String executingAssemblyFolder = Path.GetDirectoryName(executingAssemblyLocation);

        String scriptsFolder = $@"{executingAssemblyFolder}/Database/Views/{this.DatabaseEngine}";

        String[] directiories = Directory.GetDirectories(scriptsFolder);
        directiories = directiories.OrderBy(d => d).ToArray();

        foreach (String directiory in directiories) {
            String[] sqlFiles = Directory.GetFiles(directiory, "*View.sql");
            foreach (String sqlFile in sqlFiles.OrderBy(x => x)) {
                Logger.LogDebug($"About to create View [{sqlFile}]");
                String sql = File.ReadAllText(sqlFile);

                // Check here is we need to replace a Database Name
                if (sql.Contains("{DatabaseName}")) {
                    sql = sql.Replace("{DatabaseName}", this.Database.GetDbConnection().Database);
                }

                // Create the new view using the original sql from file
                await this.Database.ExecuteSqlRawAsync(sql, cancellationToken);

                Logger.LogDebug($"Created View [{sqlFile}] successfully.");
            }
        }
    }

    #endregion
}