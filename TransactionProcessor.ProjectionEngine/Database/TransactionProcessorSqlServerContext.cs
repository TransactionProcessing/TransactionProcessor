namespace TransactionProcessor.ProjectionEngine.Database;

using Microsoft.EntityFrameworkCore;
using Shared.General;

public class TransactionProcessorSqlServerContext : TransactionProcessorGenericContext
{
    public TransactionProcessorSqlServerContext() : base("SqlServer", ConfigurationReader.GetConnectionString("TransactionProcessorReadModel"))
    {
    }

    public TransactionProcessorSqlServerContext(String connectionString) : base("SqlServer", connectionString)
    {
    }

    public TransactionProcessorSqlServerContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!string.IsNullOrWhiteSpace(this.ConnectionString))
        {
            options.UseSqlServer(this.ConnectionString);
        }
    }

    protected override async Task SetIgnoreDuplicates(CancellationToken cancellationToken)
    {
        base.SetIgnoreDuplicates(cancellationToken);

        TransactionProcessorGenericContext.TablesToIgnoreDuplicates = TransactionProcessorGenericContext.TablesToIgnoreDuplicates.Select(x => $"ALTER TABLE [{x}]  REBUILD WITH (IGNORE_DUP_KEY = ON)").ToList();

        String sql = string.Join(";", TransactionProcessorGenericContext.TablesToIgnoreDuplicates);

        await this.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}