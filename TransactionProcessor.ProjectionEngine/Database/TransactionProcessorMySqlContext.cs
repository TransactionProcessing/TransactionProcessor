namespace TransactionProcessor.ProjectionEngine.Database;

using Microsoft.EntityFrameworkCore;
using Shared.General;

public class TransactionProcessorMySqlContext : TransactionProcessorGenericContext
{
    public TransactionProcessorMySqlContext() : base("MySql", ConfigurationReader.GetConnectionString("TransactionProcessorReadModel"))
    {
    }

    public TransactionProcessorMySqlContext(String connectionString) : base("MySql", connectionString)
    {
    }

    public TransactionProcessorMySqlContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!string.IsNullOrWhiteSpace(this.ConnectionString))
        {
            options.UseMySql(this.ConnectionString, ServerVersion.Parse("8.0.27")).AddInterceptors(new MySqlIgnoreDuplicatesOnInsertInterceptor());
        }
    }
}