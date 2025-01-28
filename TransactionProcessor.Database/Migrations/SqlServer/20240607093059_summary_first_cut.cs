#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class summary_first_cut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettlementSummary",
                columns: table => new
                {
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    SettlementDate = table.Column<DateTime>(type: "date", nullable: false),
                    MerchantReportingId = table.Column<int>(type: "int", nullable: false),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractProductReportingId = table.Column<int>(type: "int", nullable: false),
                    SalesValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FeeValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SalesCount = table.Column<int>(type: "int", nullable: true),
                    FeeCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "TodayTransactions",
                columns: table => new
                {
                    MerchantReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractProductReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractReportingId = table.Column<int>(type: "int", nullable: false),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorisationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAuthorised = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TransactionSource = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Hour = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "TransactionHistory",
                columns: table => new
                {
                    MerchantReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractProductReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractReportingId = table.Column<int>(type: "int", nullable: false),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorisationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAuthorised = table.Column<bool>(type: "bit", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    ResponseCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "date", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TransactionSource = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Hour = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementSummary_SettlementDate",
                table: "SettlementSummary",
                column: "SettlementDate")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementSummary_SettlementDate_MerchantReportingId_OperatorReportingId_ContractProductReportingId_IsCompleted_IsSettled",
                table: "SettlementSummary",
                columns: new[] { "SettlementDate", "MerchantReportingId", "OperatorReportingId", "ContractProductReportingId", "IsCompleted", "IsSettled" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodayTransactions_TransactionDate",
                table: "TodayTransactions",
                column: "TransactionDate")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_TodayTransactions_TransactionId",
                table: "TodayTransactions",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistory_TransactionDate",
                table: "TransactionHistory",
                column: "TransactionDate")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistory_TransactionId",
                table: "TransactionHistory",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettlementSummary");

            migrationBuilder.DropTable(
                name: "TodayTransactions");

            migrationBuilder.DropTable(
                name: "TransactionHistory");
        }
    }
}
