using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.Database.Migrations.SqlServer
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
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => new { x.EventId, x.Type })
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.CreateTable(
                name: "MerchantBalanceChangedEntry",
                columns: table => new
                {
                    AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CauseOfChangeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DebitOrCredit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantBalanceChangedEntry", x => new { x.AggregateId, x.OriginalEventId });
                });

            migrationBuilder.CreateTable(
                name: "MerchantBalanceProjectionState",
                columns: table => new
                {
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    MerchantName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositCount = table.Column<int>(type: "int", nullable: false),
                    WithdrawalCount = table.Column<int>(type: "int", nullable: false),
                    TotalDeposited = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWithdrawn = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaleCount = table.Column<int>(type: "int", nullable: false),
                    AuthorisedSales = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeclinedSales = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeCount = table.Column<int>(type: "int", nullable: false),
                    ValueOfFees = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastDeposit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastWithdrawal = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSale = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastFee = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedTransactionCount = table.Column<int>(type: "int", nullable: false),
                    CompletedTransactionCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantBalanceProjectionState", x => new { x.EstateId, x.MerchantId });
                });

            migrationBuilder.CreateTable(
                name: "voucherprojectionstate",
                columns: table => new
                {
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiryDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGenerated = table.Column<bool>(type: "bit", nullable: false),
                    IsIssued = table.Column<bool>(type: "bit", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoucherCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EstateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenerateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GenerateDate = table.Column<DateTime>(type: "date", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "date", nullable: false),
                    RedeemedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucherprojectionstate", x => x.VoucherId);
                });

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
                name: "voucherprojectionstate");

            migrationBuilder.CreateTable(
                name: "voucher",
                columns: table => new
                {
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "date", nullable: false),
                    ExpiryDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GenerateDate = table.Column<DateTime>(type: "date", nullable: false),
                    GenerateDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGenerated = table.Column<bool>(type: "bit", nullable: false),
                    IsIssued = table.Column<bool>(type: "bit", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "bit", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "date", nullable: false),
                    IssuedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedeemedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RedeemedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VoucherCode = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher", x => x.VoucherId);
                });

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
