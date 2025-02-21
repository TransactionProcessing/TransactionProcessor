using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class move_over_projection_states : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "voucher");

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => new { x.EventId, x.Type });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MerchantBalanceChangedEntry",
                columns: table => new
                {
                    AggregateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OriginalEventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CauseOfChangeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChangeAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DebitOrCredit = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Reference = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantBalanceChangedEntry", x => new { x.AggregateId, x.OriginalEventId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MerchantBalanceProjectionState",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Timestamp = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false),
                    MerchantName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AvailableBalance = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DepositCount = table.Column<int>(type: "int", nullable: false),
                    WithdrawalCount = table.Column<int>(type: "int", nullable: false),
                    TotalDeposited = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalWithdrawn = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    SaleCount = table.Column<int>(type: "int", nullable: false),
                    AuthorisedSales = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DeclinedSales = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FeeCount = table.Column<int>(type: "int", nullable: false),
                    ValueOfFees = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    LastDeposit = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastWithdrawal = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastSale = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastFee = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartedTransactionCount = table.Column<int>(type: "int", nullable: false),
                    CompletedTransactionCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantBalanceProjectionState", x => new { x.EstateId, x.MerchantId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SettlementSummary",
                columns: table => new
                {
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSettled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "date", nullable: false),
                    MerchantReportingId = table.Column<int>(type: "int", nullable: false),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractProductReportingId = table.Column<int>(type: "int", nullable: false),
                    SalesValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    FeeValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    SalesCount = table.Column<int>(type: "int", nullable: true),
                    FeeCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TodayTransactions",
                columns: table => new
                {
                    MerchantReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractProductReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractReportingId = table.Column<int>(type: "int", nullable: false),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AuthorisationCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceIdentifier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAuthorised = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ResponseCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseMessage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionReference = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    TransactionSource = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Hour = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TransactionHistory",
                columns: table => new
                {
                    MerchantReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractProductReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractReportingId = table.Column<int>(type: "int", nullable: false),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AuthorisationCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceIdentifier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAuthorised = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ResponseCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseMessage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionReference = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    TransactionSource = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Hour = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "voucherprojectionstate",
                columns: table => new
                {
                    VoucherId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiryDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsGenerated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsIssued = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientMobile = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    VoucherCode = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    GenerateDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IssuedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RedeemedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GenerateDate = table.Column<DateTime>(type: "date", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "date", nullable: false),
                    RedeemedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false),
                    Barcode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucherprojectionstate", x => x.VoucherId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementSummary_SettlementDate",
                table: "SettlementSummary",
                column: "SettlementDate");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementSummary_SettlementDate_MerchantReportingId_Operato~",
                table: "SettlementSummary",
                columns: new[] { "SettlementDate", "MerchantReportingId", "OperatorReportingId", "ContractProductReportingId", "IsCompleted", "IsSettled" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodayTransactions_TransactionDate",
                table: "TodayTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_TodayTransactions_TransactionId",
                table: "TodayTransactions",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistory_TransactionDate",
                table: "TransactionHistory",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistory_TransactionId",
                table: "TransactionHistory",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_voucherprojectionstate_TransactionId",
                table: "voucherprojectionstate",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_voucherprojectionstate_VoucherCode",
                table: "voucherprojectionstate",
                column: "VoucherCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "MerchantBalanceChangedEntry");

            migrationBuilder.DropTable(
                name: "MerchantBalanceProjectionState");

            migrationBuilder.DropTable(
                name: "SettlementSummary");

            migrationBuilder.DropTable(
                name: "TodayTransactions");

            migrationBuilder.DropTable(
                name: "TransactionHistory");

            migrationBuilder.DropTable(
                name: "voucherprojectionstate");

            migrationBuilder.CreateTable(
                name: "voucher",
                columns: table => new
                {
                    VoucherId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiryDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GenerateDate = table.Column<DateTime>(type: "date", nullable: false),
                    GenerateDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsGenerated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsIssued = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "date", nullable: false),
                    IssuedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientEmail = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientMobile = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RedeemedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RedeemedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Value = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    VoucherCode = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher", x => x.VoucherId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_voucher_TransactionId",
                table: "voucher",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_voucher_VoucherCode",
                table: "voucher",
                column: "VoucherCode");
        }
    }
}
