namespace TransactionProcessor.ProjectionEngine.Database.Database;

using Microsoft.EntityFrameworkCore;
using TransactionProcessing.Shared.General;

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
            options.UseMySQL(this.ConnectionString).AddInterceptors(new MySqlIgnoreDuplicatesOnInsertInterceptor());
        }
    }
}
