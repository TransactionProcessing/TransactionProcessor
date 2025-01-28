#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "calendar",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DayOfWeek = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DayOfWeekNumber = table.Column<int>(type: "int", nullable: false),
                    DayOfWeekShort = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MonthNameLong = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MonthNameShort = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MonthNumber = table.Column<int>(type: "int", nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: true),
                    WeekNumberString = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    YearWeekNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar", x => x.Date);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contract",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OperatorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract", x => new { x.EstateId, x.OperatorId, x.ContractId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contractproduct",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DisplayText = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<decimal>(type: "decimal(65,30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractproduct", x => new { x.EstateId, x.ContractId, x.ProductId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "contractproducttransactionfee",
                columns: table => new
                {
                    ContractId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ProductId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionFeeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CalculationType = table.Column<int>(type: "int", nullable: false),
                    FeeType = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractproducttransactionfee", x => new { x.EstateId, x.ContractId, x.ProductId, x.TransactionFeeId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estate",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reference = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estate", x => x.EstateId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estateoperator",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OperatorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequireCustomMerchantNumber = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequireCustomTerminalNumber = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estateoperator", x => new { x.EstateId, x.OperatorId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estatesecurityuser",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SecurityUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estatesecurityuser", x => new { x.SecurityUserId, x.EstateId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "file",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileImportLogId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileLocation = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileReceivedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file", x => new { x.EstateId, x.FileImportLogId, x.FileId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fileimportlog",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileImportLogId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImportLogDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileimportlog", x => new { x.EstateId, x.FileImportLogId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fileimportlogfile",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileImportLogId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FilePath = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileUploadedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OriginalFileName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileimportlogfile", x => new { x.EstateId, x.FileImportLogId, x.FileId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fileline",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    FileLineData = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fileline", x => new { x.EstateId, x.FileId, x.LineNumber });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "merchant",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SettlementSchedule = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastStatementGenerated = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchant", x => new { x.EstateId, x.MerchantId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "merchantaddress",
                columns: table => new
                {
                    AddressId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AddressLine1 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddressLine2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddressLine3 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddressLine4 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PostalCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Region = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Town = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantaddress", x => new { x.EstateId, x.MerchantId, x.AddressId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "merchantcontact",
                columns: table => new
                {
                    ContactId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantcontact", x => new { x.EstateId, x.MerchantId, x.ContactId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "merchantdevice",
                columns: table => new
                {
                    DeviceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantdevice", x => new { x.EstateId, x.MerchantId, x.DeviceId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "merchantoperator",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OperatorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TerminalNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantoperator", x => new { x.EstateId, x.MerchantId, x.OperatorId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "merchantsecurityuser",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SecurityUserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantsecurityuser", x => new { x.EstateId, x.MerchantId, x.SecurityUserId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "merchantsettlementfee",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FeeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SettlementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CalculatedValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FeeCalculatedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FeeValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    IsSettled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_merchantsettlementfee", x => new { x.EstateId, x.SettlementId, x.TransactionId, x.FeeId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reconciliation",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DeviceIdentifier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsAuthorised = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResponseCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseMessage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionCount = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    TransactionValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reconciliation", x => x.TransactionId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "responsecodes",
                columns: table => new
                {
                    ResponseCode = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_responsecodes", x => x.ResponseCode);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "settlement",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SettlementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement", x => new { x.EstateId, x.SettlementId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "statementheader",
                columns: table => new
                {
                    StatementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StatementCreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StatementGeneratedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statementheader", x => x.StatementId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "statementline",
                columns: table => new
                {
                    StatementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActivityDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ActivityDescription = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    OutAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statementline", x => new { x.StatementId, x.TransactionId, x.ActivityDateTime, x.ActivityType });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transaction",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AuthorisationCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContractId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DeviceIdentifier = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAuthorised = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResponseCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseMessage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionReference = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    TransactionType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionSource = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction", x => new { x.EstateId, x.MerchantId, x.TransactionId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transactionadditionalrequestdata",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Amount = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerAccountNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionadditionalrequestdata", x => new { x.EstateId, x.MerchantId, x.TransactionId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transactionadditionalresponsedata",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionadditionalresponsedata", x => x.TransactionId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transactionfee",
                columns: table => new
                {
                    FeeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CalculatedValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CalculationType = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FeeType = table.Column<int>(type: "int", nullable: false),
                    FeeValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionfee", x => new { x.TransactionId, x.FeeId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "uvwFileImportLogView",
                columns: table => new
                {
                    FileImportLogId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImportLogDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ImportLogDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ImportLogTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    FileCount = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "uvwFileView",
                columns: table => new
                {
                    FileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileReceivedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FileReceivedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FileReceivedTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LineCount = table.Column<int>(type: "int", nullable: false),
                    PendingCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "uvwTransactionsView",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DayOfWeek = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MonthNumber = table.Column<int>(type: "int", nullable: false),
                    YearNumber = table.Column<int>(type: "int", nullable: false),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsAuthorised = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TransactionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "voucher",
                columns: table => new
                {
                    VoucherId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsGenerated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsIssued = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientEmail = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientMobile = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    VoucherCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GenerateDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IssuedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RedeemedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher", x => x.VoucherId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
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
