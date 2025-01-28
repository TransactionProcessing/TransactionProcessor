#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calendar",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DayOfWeekNumber = table.Column<int>(type: "int", nullable: false),
                    DayOfWeekShort = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthNameLong = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthNameShort = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthNumber = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: true),
                    WeekNumberString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    YearWeekNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "contract",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract", x => new { x.EstateId, x.OperatorId, x.ContractId });
                });

            migrationBuilder.CreateTable(
                name: "contractproduct",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractproduct", x => new { x.EstateId, x.ContractId, x.ProductId });
                });

            migrationBuilder.CreateTable(
                name: "contractproducttransactionfee",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionFeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculationType = table.Column<int>(type: "int", nullable: false),
                    FeeType = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractproducttransactionfee", x => new { x.EstateId, x.ContractId, x.ProductId, x.TransactionFeeId });
                });

            migrationBuilder.CreateTable(
                name: "estate",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estate", x => x.EstateId);
                });

            migrationBuilder.CreateTable(
                name: "estateoperator",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequireCustomMerchantNumber = table.Column<bool>(type: "bit", nullable: false),
                    RequireCustomTerminalNumber = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estateoperator", x => new { x.EstateId, x.OperatorId });
                });

            migrationBuilder.CreateTable(
                name: "estatesecurityuser",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecurityUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estatesecurityuser", x => new { x.SecurityUserId, x.EstateId });
                });

            migrationBuilder.CreateTable(
                name: "file",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileImportLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileReceivedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file", x => new { x.EstateId, x.FileImportLogId, x.FileId });
                });

            migrationBuilder.CreateTable(
                name: "fileimportlog",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileImportLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportLogDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileimportlog", x => new { x.EstateId, x.FileImportLogId });
                });

            migrationBuilder.CreateTable(
                name: "fileimportlogfile",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileImportLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileUploadedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileimportlogfile", x => new { x.EstateId, x.FileImportLogId, x.FileId });
                });

            migrationBuilder.CreateTable(
                name: "fileline",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    FileLineData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileline", x => new { x.EstateId, x.FileId, x.LineNumber });
                });

            migrationBuilder.CreateTable(
                name: "merchant",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SettlementSchedule = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastStatementGenerated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchant", x => new { x.EstateId, x.MerchantId });
                });

            migrationBuilder.CreateTable(
                name: "merchantaddress",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLine4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Town = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantaddress", x => new { x.EstateId, x.MerchantId, x.AddressId });
                });

            migrationBuilder.CreateTable(
                name: "merchantcontact",
                columns: table => new
                {
                    ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantcontact", x => new { x.EstateId, x.MerchantId, x.ContactId });
                });

            migrationBuilder.CreateTable(
                name: "merchantdevice",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantdevice", x => new { x.EstateId, x.MerchantId, x.DeviceId });
                });

            migrationBuilder.CreateTable(
                name: "merchantoperator",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TerminalNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantoperator", x => new { x.EstateId, x.MerchantId, x.OperatorId });
                });

            migrationBuilder.CreateTable(
                name: "merchantsecurityuser",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SecurityUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantsecurityuser", x => new { x.EstateId, x.MerchantId, x.SecurityUserId });
                });

            migrationBuilder.CreateTable(
                name: "merchantsettlementfee",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeCalculatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FeeValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantsettlementfee", x => new { x.EstateId, x.SettlementId, x.TransactionId, x.FeeId });
                });

            migrationBuilder.CreateTable(
                name: "reconciliation",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAuthorised = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionCount = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TransactionValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reconciliation", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "responsecodes",
                columns: table => new
                {
                    ResponseCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_responsecodes", x => x.ResponseCode);
                });

            migrationBuilder.CreateTable(
                name: "settlement",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement", x => new { x.EstateId, x.SettlementId });
                });

            migrationBuilder.CreateTable(
                name: "statementheader",
                columns: table => new
                {
                    StatementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatementCreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatementGeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statementheader", x => x.StatementId);
                });

            migrationBuilder.CreateTable(
                name: "statementline",
                columns: table => new
                {
                    StatementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statementline", x => new { x.StatementId, x.TransactionId, x.ActivityDateTime, x.ActivityType });
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorisationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAuthorised = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionSource = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction", x => new { x.EstateId, x.MerchantId, x.TransactionId });
                });

            migrationBuilder.CreateTable(
                name: "transactionadditionalrequestdata",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionadditionalrequestdata", x => new { x.EstateId, x.MerchantId, x.TransactionId });
                });

            migrationBuilder.CreateTable(
                name: "transactionadditionalresponsedata",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionadditionalresponsedata", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "transactionfee",
                columns: table => new
                {
                    FeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CalculationType = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeType = table.Column<int>(type: "int", nullable: false),
                    FeeValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionfee", x => new { x.TransactionId, x.FeeId });
                });

            migrationBuilder.CreateTable(
                name: "uvwFileImportLogView",
                columns: table => new
                {
                    FileImportLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportLogDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImportLogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImportLogTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    FileCount = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "uvwFileView",
                columns: table => new
                {
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileReceivedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileReceivedTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LineCount = table.Column<int>(type: "int", nullable: false),
                    PendingCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "uvwTransactionsView",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthNumber = table.Column<int>(type: "int", nullable: false),
                    YearNumber = table.Column<int>(type: "int", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAuthorised = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "voucher",
                columns: table => new
                {
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGenerated = table.Column<bool>(type: "bit", nullable: false),
                    IsIssued = table.Column<bool>(type: "bit", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientMobile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoucherCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenerateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher", x => x.VoucherId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calendar");

            migrationBuilder.DropTable(
                name: "contract");

            migrationBuilder.DropTable(
                name: "contractproduct");

            migrationBuilder.DropTable(
                name: "contractproducttransactionfee");

            migrationBuilder.DropTable(
                name: "estate");

            migrationBuilder.DropTable(
                name: "estateoperator");

            migrationBuilder.DropTable(
                name: "estatesecurityuser");

            migrationBuilder.DropTable(
                name: "file");

            migrationBuilder.DropTable(
                name: "fileimportlog");

            migrationBuilder.DropTable(
                name: "fileimportlogfile");

            migrationBuilder.DropTable(
                name: "fileline");

            migrationBuilder.DropTable(
                name: "merchant");

            migrationBuilder.DropTable(
                name: "merchantaddress");

            migrationBuilder.DropTable(
                name: "merchantcontact");

            migrationBuilder.DropTable(
                name: "merchantdevice");

            migrationBuilder.DropTable(
                name: "merchantoperator");

            migrationBuilder.DropTable(
                name: "merchantsecurityuser");

            migrationBuilder.DropTable(
                name: "merchantsettlementfee");

            migrationBuilder.DropTable(
                name: "reconciliation");

            migrationBuilder.DropTable(
                name: "responsecodes");

            migrationBuilder.DropTable(
                name: "settlement");

            migrationBuilder.DropTable(
                name: "statementheader");

            migrationBuilder.DropTable(
                name: "statementline");

            migrationBuilder.DropTable(
                name: "transaction");

            migrationBuilder.DropTable(
                name: "transactionadditionalrequestdata");

            migrationBuilder.DropTable(
                name: "transactionadditionalresponsedata");

            migrationBuilder.DropTable(
                name: "transactionfee");

            migrationBuilder.DropTable(
                name: "uvwFileImportLogView");

            migrationBuilder.DropTable(
                name: "uvwFileView");

            migrationBuilder.DropTable(
                name: "uvwTransactionsView");

            migrationBuilder.DropTable(
                name: "voucher");
        }
    }
}
