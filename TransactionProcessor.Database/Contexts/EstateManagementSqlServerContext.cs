using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Shared.General;

namespace TransactionProcessor.Database.Contexts;

public class EstateManagementSqlServerContext : EstateManagementGenericContext
{
    public EstateManagementSqlServerContext() : base("SqlServer", ConfigurationReader.GetConnectionString("EstateReportingReadModel"))
    {
    }

    public EstateManagementSqlServerContext(String connectionString) : base("SqlServer", connectionString)
    {
    }

    public EstateManagementSqlServerContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!string.IsNullOrWhiteSpace(this.ConnectionString))
        {
            options.UseSqlServer(this.ConnectionString);
        }

        options.UseExceptionProcessor();
    }

    protected override async Task SetIgnoreDuplicates(CancellationToken cancellationToken)
    {
        base.SetIgnoreDuplicates(cancellationToken);

        var tableList = EstateManagementGenericContext.TablesToIgnoreDuplicates.Select(x => $"ALTER TABLE [{x}]  REBUILD WITH (IGNORE_DUP_KEY = ON)").ToList();

        String sql = string.Join(";", tableList);

        await this.Database.ExecuteSqlRawAsync(sql, cancellationToken);

        //var tableList2 = new List<String>();
        //tableList2.AddRange(EstateManagementGenericContext.TablesToIgnoreDuplicates.Select(x => $"DECLARE @TableName NVARCHAR(128);\r\nDECLARE @IndexScript NVARCHAR(MAX) = '';\r\nSET @TableName = '{x}';\r\n\r\nSELECT @IndexScript = @IndexScript + 'ALTER INDEX ' + QUOTENAME(i.name) + ' ON ' + QUOTENAME(OBJECT_SCHEMA_NAME(i.object_id)) + '.' + QUOTENAME(OBJECT_NAME(i.object_id)) + ' SET ( IGNORE_DUP_KEY = ON );'\r\nFROM sys.indexes AS i\r\nWHERE i.object_id = OBJECT_ID(@TableName) AND i.index_id > 0 and is_primary_key = 0 and is_unique = 1;\r\n\r\nEXEC sp_executesql @IndexScript;").ToList());

        //foreach (String sql2 in tableList2){
        //    await this.Database.ExecuteSqlRawAsync(sql2, cancellationToken);
        //}
    }
}