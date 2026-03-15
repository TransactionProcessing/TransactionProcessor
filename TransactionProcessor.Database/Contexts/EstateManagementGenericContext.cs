using EntityFramework.Exceptions.Common;
using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.Exceptions;
using Shared.Logger;
using SimpleResults;
using System.Reflection;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Database.Entities.Summary;
using TransactionProcessor.Database.ViewEntities;
using TransactionProcessor.ProjectionEngine.Database.Database.Entities;
using TransactionProcessor.ProjectionEngine.Database.Database.ViewEntities;
using File = TransactionProcessor.Database.Entities.File;

namespace TransactionProcessor.Database.Contexts;

public class EstateManagementContext : DbContext
{
    #region Fields

    protected readonly String ConnectionString;

    protected static List<String> TablesToIgnoreDuplicates = new List<String>();

    #endregion

    #region Constructors

    public EstateManagementContext(String connectionString)
    {
        this.ConnectionString = connectionString;
    }

    public EstateManagementContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

    #endregion

    #region Properties

    public DbSet<MerchantOpeningHours> MerchantOpeningHours { get; set; }
    public DbSet<MerchantBalanceChangedEntry> MerchantBalanceChangedEntry { get; set; }

    public DbSet<MerchantBalanceHistoryViewEntry> MerchantBalanceHistoryViewEntry { get; set; }

    public DbSet<MerchantBalanceProjectionState> MerchantBalanceProjectionState { get; set; }

    public DbSet<Calendar> Calendar { get; set; }

    public DbSet<ContractProduct> ContractProducts { get; set; }

    public DbSet<ContractProductTransactionFee> ContractProductTransactionFees { get; set; }

    public DbSet<Entities.Contract> Contracts { get; set; }

    public DbSet<Operator> Operators { get; set; }

    public DbSet<Estate> Estates { get; set; }

    public DbSet<EstateOperator> EstateOperators { get; set; }

    public DbSet<EstateSecurityUser> EstateSecurityUsers { get; set; }
    
    public virtual DbSet<FileImportLogFile> FileImportLogFiles { get; set; }
    
    public virtual DbSet<FileImportLog> FileImportLogs { get; set; }
    
    public virtual DbSet<FileLine> FileLines { get; set; }

    public virtual DbSet<File> Files { get; set; }
    public DbSet<Float> Floats { get; set; }
    public DbSet<FloatActivity> FloatActivity { get; set; }

    public DbSet<MerchantAddress> MerchantAddresses { get; set; }

    public DbSet<MerchantContact> MerchantContacts { get; set; }

    public DbSet<MerchantDevice> MerchantDevices { get; set; }

    public DbSet<MerchantOperator> MerchantOperators { get; set; }
    
    public DbSet<Merchant> Merchants { get; set; }

    public DbSet<MerchantSecurityUser> MerchantSecurityUsers { get; set; }

    public DbSet<MerchantSettlementFee> MerchantSettlementFees { get; set; }
    
    public DbSet<Reconciliation> Reconciliations { get; set; }

    public DbSet<ResponseCodes> ResponseCodes { get; set; }

    public DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<SettlementView> SettlementsView { get; set; }

    public DbSet<StatementHeader> StatementHeaders { get; set; }

    public DbSet<StatementLine> StatementLines { get; set; }
    
    public DbSet<Transaction> Transactions { get; set; }

    public DbSet<TransactionAdditionalRequestData> TransactionsAdditionalRequestData { get; set; }

    public DbSet<TransactionAdditionalResponseData> TransactionsAdditionalResponseData { get; set; }

    public DbSet<VoucherProjectionState> VoucherProjectionStates { get; set; }

    public DbSet<MerchantContract> MerchantContracts { get; set; }

    public DbSet<SettlementSummary> SettlementSummary { get; set; }
    public DbSet<TodayTransaction> TodayTransactions { get; set; }
    public DbSet<TransactionHistory> TransactionHistory { get; set; }

    public DbSet<TransactionTimings> TransactionTimings { get; set; }

    public DbSet<Event> Events { get; set; }

    #endregion

    #region Methods

    private async Task CreateStoredProcedures(CancellationToken cancellationToken)
    {
        String executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        String executingAssemblyFolder = Path.GetDirectoryName(executingAssemblyLocation);

        String scriptsFolder = $@"{executingAssemblyFolder}/StoredProcedures";

        String[] directories = Directory.GetDirectories(scriptsFolder);
        if (directories.Length == 0)
        {
            var list = new List<string> { scriptsFolder };
            directories = list.ToArray();
        }
        directories = directories.OrderBy(d => d).ToArray();

        foreach (String directiory in directories)
        {
            String[] sqlFiles = Directory.GetFiles(directiory, "*.sql");
            foreach (String sqlFile in sqlFiles.OrderBy(x => x))
            {
                Logger.LogInformation($"About to create Stored Procedure [{sqlFile}]");
                String sql = System.IO.File.ReadAllText(sqlFile);

                // Check here is we need to replace a Database Name
                if (sql.Contains("{DatabaseName}"))
                {
                    sql = sql.Replace("{DatabaseName}", this.Database.GetDbConnection().Database);
                }

                // Create the new view using the original sql from file
                await this.Database.ExecuteSqlRawAsync(sql, cancellationToken);

                Logger.LogWarning($"Created Stored Procedure [{sqlFile}] successfully.");
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!string.IsNullOrWhiteSpace(this.ConnectionString))
        {
            options.UseSqlServer(this.ConnectionString);
        }

        options.UseExceptionProcessor();
    }

    private async Task CreateViews(CancellationToken cancellationToken)
    {
        String executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        String executingAssemblyFolder = Path.GetDirectoryName(executingAssemblyLocation);

        String scriptsFolder = $@"{executingAssemblyFolder}/Views";

        String[] directiories = Directory.GetDirectories(scriptsFolder, "", SearchOption.AllDirectories);
        directiories = directiories.OrderBy(d => d).ToArray();

        foreach (String directiory in directiories)
        {
            String[] sqlFiles = Directory.GetFiles(directiory, "*View.sql");
            foreach (String sqlFile in sqlFiles.OrderBy(x => x))
            {
                Logger.LogInformation($"About to create View [{sqlFile}]");
                String sql = System.IO.File.ReadAllText(sqlFile);

                // Check here is we need to replace a Database Name
                if (sql.Contains("{DatabaseName}"))
                {
                    sql = sql.Replace("{DatabaseName}", this.Database.GetDbConnection().Database);
                }

                // Create the new view using the original sql from file
                await this.Database.ExecuteSqlRawAsync(sql, cancellationToken);

                Logger.LogWarning($"Created View [{sqlFile}] successfully.");
            }
        }
    }

    private async Task SeedStandingData(CancellationToken cancellationToken)
    {
        String executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        String executingAssemblyFolder = Path.GetDirectoryName(executingAssemblyLocation);

        String scriptsFolder = $@"{executingAssemblyFolder}/SeedingScripts";

        String[] sqlFiles = Directory.GetFiles(scriptsFolder, "*.sql");
        foreach (String sqlFile in sqlFiles.OrderBy(x => x))
        {
            Logger.LogDebug($"About to create View [{sqlFile}]");
            String sql = System.IO.File.ReadAllText(sqlFile);

            // Check here is we need to replace a Database Name
            if (sql.Contains("{DatabaseName}"))
            {
                sql = sql.Replace("{DatabaseName}", this.Database.GetDbConnection().Database);
            }

            // Create the new view using the original sql from file
            await this.Database.ExecuteSqlRawAsync(sql, cancellationToken);

            Logger.LogWarning($"Run Seeding Script [{sqlFile}] successfully.");
        }
    }

    private async Task SetDbInSimpleMode(CancellationToken cancellationToken)
    {
        var dbName = this.Database.GetDbConnection().Database;

        var connection = this.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        // 1. Check current recovery model
        await using var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = @"
SELECT recovery_model_desc
FROM sys.databases
WHERE name = @dbName;
";
        var param = checkCommand.CreateParameter();
        param.ParameterName = "@dbName";
        param.Value = dbName;
        checkCommand.Parameters.Add(param);

        var result = await checkCommand.ExecuteScalarAsync(cancellationToken);
        var currentRecoveryModel = result?.ToString();

        if (currentRecoveryModel != "SIMPLE")
        {
            // 2. Alter database outside transaction
            await using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $@"
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE;
ALTER DATABASE [{dbName}] SET MULTI_USER;
";
            // Execute outside EF transaction
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }




    public static Boolean IsDuplicateInsertsIgnored(String tableName) =>
        EstateManagementContext.TablesToIgnoreDuplicates.Contains(tableName.Trim(), StringComparer.InvariantCultureIgnoreCase);

    public virtual async Task MigrateAsync(CancellationToken cancellationToken)
    {
        if (this.Database.IsSqlServer())
        {
            await this.Database.MigrateAsync(cancellationToken);
            await this.SetIgnoreDuplicates(cancellationToken);
            await this.CreateViews(cancellationToken);
            await this.SeedStandingData(cancellationToken);
            await this.CreateStoredProcedures(cancellationToken);
            await this.SetDbInSimpleMode(cancellationToken);

        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.SetupResponseCodes()
                    .SetupEstate()
                    .SetupEstateSecurityUser()
                    .SetupMerchant()
                    .SetupMerchantAddress()
                    .SetupMerchantContact()
                    .SetupMerchantDevice()
                    .SetupMerchantDevice()
                    .SetupMerchantOperator()
                    .SetupMerchantSecurityUser()
                    .SetupContract()
                    .SetupContractProduct()
                    .SetupContractProductTransactionFee()
                    .SetupTransaction()
                    .SetupTransactionAdditionalResponseData()
                    .SetupTransactionAdditionalRequestData()
                    .SetupSettlement()
                    .SetupMerchantSettlementFee()
                    .SetupFile()
                    .SetupFileImportLog()
                    .SetupFileImportLogFile()
                    .SetupFileLine()
                    .SetupStatementHeader()
                    .SetupStatementLine()
                    .SetupReconciliation()
                    .SetupVoucher()
                    .SetupMerchantContract()
                    .SetupFloat()
                    .SetupFloatActivity()
                    .SetupOperator()
                    .SetupSettlementSummary()
                    .SetupTransactionHistory()
                    .SetupTodaysTransactions()
                    .SetupTransactionTimings()
                    .SetupEstateOperator()
                    .SetupMerchantOpeningHours();
        
        modelBuilder.SetupViewEntities();

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

    protected virtual async Task SetIgnoreDuplicates(CancellationToken cancellationToken)
    {
        EstateManagementContext.TablesToIgnoreDuplicates = new List<String> {
                                                                                      nameof(this.ResponseCodes),
                                                                                      nameof(this.MerchantBalanceProjectionState),
                                                                                  };

        var tableList = EstateManagementContext.TablesToIgnoreDuplicates.Select(x => $"ALTER TABLE [{x}]  REBUILD WITH (IGNORE_DUP_KEY = ON)").ToList();

        String sql = string.Join(";", tableList);

        await this.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public new async Task<Result> SaveChangesAsync(CancellationToken cancellationToken) {
        try {
            await base.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) {
            return Result.Failure(ex.GetExceptionMessages());
        }
    }

    public virtual async Task<Result> SaveChangesWithDuplicateHandling(CancellationToken cancellationToken)
    {
        try
        {
            await base.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (UniqueConstraintException uex)
        {
            // Swallow the error
            Logger.LogInformation(BuildUniqueConstraintExceptionLogMessage(uex));
            return Result.Success();
        }
    }

    private static String BuildUniqueConstraintExceptionLogMessage(UniqueConstraintException uex) {
        String constraintName = "N/A";
        if (String.IsNullOrEmpty(uex.ConstraintName) == false) {
            constraintName = uex.ConstraintName;
        }
        String constraintProperties = "N/A";
        if (uex.ConstraintProperties != null) {
            constraintProperties = String.Join(",", uex.ConstraintProperties);
        }
        return $"Unique Constraint Exception. Message [{uex.Message}] Inner Exception [{uex.InnerException.Message}]";

    }


    #endregion
}

public static class EstateManagementContextExtensions
{
    public static async Task<Result<Estate>> LoadEstate(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid estateId = DomainEventHelper.GetEstateId(domainEvent);
        IQueryable<Estate> query = context.Estates.Where(e => e.EstateId == estateId);
        Result<Estate> estateResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting estate with Id {estateId}");

        return estateResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Estate not found with Id {estateId}"),
            _ => estateResult
        };
    }

    public static async Task<Result<Operator>> LoadOperator(this EstateManagementContext context, Guid operatorId, CancellationToken cancellationToken)
    {
        IQueryable<Operator> query = context.Operators.Where(e => e.OperatorId == operatorId);
        Result<Operator> operatorResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting operator with Id {operatorId}");

        return operatorResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Operator not found with Id {operatorId}"),
            _ => operatorResult
        };
    }

    public static async Task<Result<Operator>> LoadOperator(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid operatorId = DomainEventHelper.GetOperatorId(domainEvent);
        Result<Operator> loadOperatorResult = await context.LoadOperator(operatorId, cancellationToken);

        return loadOperatorResult;
    }

    public static async Task<Result<MerchantDevice>> LoadOriginalMerchantDevice(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid merchantId = DomainEventHelper.GetMerchantId(domainEvent);
        Guid deviceId = DomainEventHelper.GetOriginalDeviceId(domainEvent);
        IQueryable<MerchantDevice> query = context.MerchantDevices.Where(d => d.DeviceId == deviceId &&
                                                                              d.MerchantId == merchantId);
        Result<MerchantDevice> deviceResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting original device with Id {deviceId} for Merchant {merchantId}");

        return deviceResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Original Device Id {deviceId} not found for Merchant {merchantId}"),
            _ => deviceResult
        };
    }

    public static async Task<Result<MerchantDevice>> LoadMerchantDevice(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid merchantId = DomainEventHelper.GetMerchantId(domainEvent);
        Guid deviceId = DomainEventHelper.GetDeviceId(domainEvent);
        IQueryable<MerchantDevice> query = context.MerchantDevices.Where(d => d.DeviceId == deviceId &&
                                                                              d.MerchantId == merchantId);
        Result<MerchantDevice> deviceResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting device with Id {deviceId} for Merchant {merchantId}");

        return deviceResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Device Id {deviceId} not found for Merchant {merchantId}"),
            _ => deviceResult
        };
    }

    public static async Task<Result<File>> LoadFile(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid fileId = DomainEventHelper.GetFileId(domainEvent);
        IQueryable<File> query = context.Files.Where(e => e.FileId == fileId);
        Result<File> fileResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting file with Id {fileId}");

        return fileResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"File not found with Id {fileId}"),
            _ => fileResult
        };
    }

    public static async Task<Result<Merchant>> LoadMerchant(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid merchantId = DomainEventHelper.GetMerchantId(domainEvent);
        
        IQueryable<Merchant> query = context.Merchants.Where(e => e.MerchantId == merchantId);
        Result<Merchant> merchantResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting merchant with Id {merchantId}");
        return merchantResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Merchant not found with Id {merchantId}"),
            _ => merchantResult
        };
    }

    public static async Task<Result<MerchantAddress>> LoadMerchantAddress(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid merchantId = DomainEventHelper.GetMerchantId(domainEvent);
        Guid addressId = DomainEventHelper.GetAddressId(domainEvent);

        IQueryable<MerchantAddress> query = context.MerchantAddresses.Where(e => e.MerchantId == merchantId && e.AddressId == addressId);
        Result<MerchantAddress> merchantAddressResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting merchant address with Id {addressId} for merchant {merchantId}");
        return merchantAddressResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Merchant Address {addressId} not found with merchant Id {merchantId}"),
            _ => merchantAddressResult
        };
    }

    public static async Task<Result<MerchantContact>> LoadMerchantContact(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid merchantId = DomainEventHelper.GetMerchantId(domainEvent);

        Guid contactId = DomainEventHelper.GetContactId(domainEvent);
        IQueryable<MerchantContact> query = context.MerchantContacts.Where(e => e.MerchantId == merchantId && e.ContactId == contactId);
        Result<MerchantContact> merchantContactResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting merchant contact with Id {contactId} for merchant {merchantId}");
        return merchantContactResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Merchant Contact {contactId} not found with merchant Id {merchantId}"),
            _ => merchantContactResult        
        };
    }

    public static async Task<Result<MerchantOpeningHours>> LoadMerchantOpeningHours(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid merchantId = DomainEventHelper.GetMerchantId(domainEvent);

        IQueryable<MerchantOpeningHours> query = context.MerchantOpeningHours.Where(e => e.MerchantId == merchantId);
        Result<MerchantOpeningHours> merchantOpeningHoursResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting merchant opening hours for merchant {merchantId}");
        return merchantOpeningHoursResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Merchant Opening Hours not found for merchant Id {merchantId}"),
            _ => merchantOpeningHoursResult
        };
    }

    public static async Task<Result<Reconciliation>> LoadReconcilation(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid transactionId = DomainEventHelper.GetTransactionId(domainEvent);
        IQueryable<Reconciliation> query = context.Reconciliations.Where(t => t.TransactionId == transactionId);
        Result<Reconciliation> reconciliationResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting reconciliation with Id {transactionId}");
        return reconciliationResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Reconciliation not found with Id {transactionId}"),
            _ => reconciliationResult
        };
    }

    public static async Task<Result<Settlement>> LoadSettlement(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid settlementId = DomainEventHelper.GetSettlementId(domainEvent);
        IQueryable<Settlement> query = context.Settlements.Where(e => e.SettlementId == settlementId);
        Result<Settlement> settlementResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting settlement with Id {settlementId}");
        return settlementResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Settlement not found with Id {settlementId}"),
            _ => settlementResult
        };
    }

    public static async Task<Result<StatementHeader>> LoadStatementHeader(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid statementHeaderId = DomainEventHelper.GetStatementHeaderId(domainEvent);
        IQueryable<StatementHeader> query = context.StatementHeaders.Where(e => e.StatementId == statementHeaderId);
        Result<StatementHeader> statementHeaderResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting statement header with Id {statementHeaderId}");
        return statementHeaderResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Statement Header not found with Id {statementHeaderId}"),
            _ => statementHeaderResult
        };
    }

    public static async Task<Result<Transaction>> LoadTransaction(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid transactionId = DomainEventHelper.GetTransactionId(domainEvent);
        IQueryable<Transaction> query = context.Transactions.Where(e => e.TransactionId == transactionId);
        Result<Transaction> transactionResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting transaction with Id {transactionId}");
        return transactionResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Transaction not found with Id {transactionId}"),
            _ => transactionResult
        };
    }

    //public static async Task<Result<Voucher>> LoadVoucher(this EstateManagementGenericContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    //{
    //    Guid voucherId = DomainEventHelper.GetVoucherId(domainEvent);
    //    Voucher voucher = await context.Vouchers.SingleOrDefaultAsync(v => v.VoucherId == voucherId, cancellationToken);

    //    return voucher switch
    //    {
    //        null => Result.NotFound($"Voucher not found with Id {voucherId}"),
    //        _ => Result.Success(voucher)
    //    };
    //}

    public static async Task<Result<Entities.Contract>> LoadContract(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid contractId = DomainEventHelper.GetContractId(domainEvent);
        IQueryable<Entities.Contract> query = context.Contracts.Where(e => e.ContractId == contractId);
        Result<Entities.Contract> contractResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting contract with Id {contractId}");
        return contractResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Contract not found with Id {contractId}"),
            _ => contractResult
        };
    }

    public static async Task<Result<ContractProductTransactionFee>> LoadContractProductTransactionFee(this EstateManagementContext context, IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        Guid contractProductTransactionFeeId = DomainEventHelper.GetContractProductTransactionFeeId(domainEvent);
        IQueryable<ContractProductTransactionFee> query = context.ContractProductTransactionFees.Where(e => e.ContractProductTransactionFeeId == contractProductTransactionFeeId);
        Result<ContractProductTransactionFee> contractProductTransactionFeeResult = await DbQueryHelpers.ExecuteQuerySafeSingleOrDefault(query, cancellationToken, $"Error getting contract product transaction fee with Id {contractProductTransactionFeeId}");
        return contractProductTransactionFeeResult.Status switch
        {
            ResultStatus.NotFound => Result.NotFound($"Contract Product Transaction Fee not found with Id {contractProductTransactionFeeId}"),
            _ => contractProductTransactionFeeResult
        };
    }
}

public static class DomainEventHelper
{
    #region Methods

    public static Guid GetContractId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "ContractId");

    public static Guid GetContractProductId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "ProductId");

    public static Guid GetContractProductTransactionFeeId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "FeeId");

    public static Guid GetEstateId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "EstateId");
    public static Guid GetOperatorId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "OperatorId");

    public static Guid GetFileId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "FileId");

    public static Guid GetFileImportLogId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "FileImportLogId");
    public static Guid GetOriginalDeviceId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "OriginalDeviceId");
    public static Guid GetDeviceId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "DeviceId");
    public static Guid GetMerchantId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "MerchantId");
    public static Guid GetAddressId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "AddressId");

    public static Guid GetContactId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "ContactId");

    public static T GetProperty<T>(IDomainEvent domainEvent, String propertyName)
    {
        try
        {
            var f = domainEvent.GetType()
                               .GetProperties()
                               .SingleOrDefault(p => p.Name == propertyName);

            if (f != null)
            {
                return (T)f.GetValue(domainEvent);
            }
        }
        catch
        {
            // ignored
        }

        return default(T);
    }

    public static T GetPropertyIgnoreCase<T>(IDomainEvent domainEvent, String propertyName)
    {
        try
        {
            var f = domainEvent.GetType()
                               .GetProperties()
                               .SingleOrDefault(p => String.Compare(p.Name, propertyName, StringComparison.CurrentCultureIgnoreCase) == 0);

            if (f != null)
            {
                return (T)f.GetValue(domainEvent);
            }
        }
        catch
        {
            // ignored
        }

        return default(T);
    }

    public static Guid GetSettlementId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "SettlementId");

    public static Guid GetStatementHeaderId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "MerchantStatementId");

    public static Guid GetTransactionId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "TransactionId");

    public static Guid GetVoucherId(IDomainEvent domainEvent) => DomainEventHelper.GetProperty<Guid>(domainEvent, "VoucherId");

    public static Boolean HasProperty(IDomainEvent domainEvent,
                                      String propertyName)
    {
        PropertyInfo propertyInfo = domainEvent.GetType()
                                               .GetProperties()
                                               .SingleOrDefault(p => p.Name == propertyName);

        return propertyInfo != null;
    }

    #endregion
}