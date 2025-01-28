#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class handleSettlementProcessingStartedEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactionfee");

            migrationBuilder.AddColumn<bool>(
                name: "ProcessingStarted",
                table: "settlement",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingStartedDateTIme",
                table: "settlement",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessingStarted",
                table: "settlement");

            migrationBuilder.DropColumn(
                name: "ProcessingStartedDateTIme",
                table: "settlement");

            migrationBuilder.CreateTable(
                name: "transactionfee",
                columns: table => new
                {
                    TransactionReportingId = table.Column<int>(type: "int", nullable: false),
                    TransactionFeeReportingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CalculatedValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CalculationType = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FeeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FeeType = table.Column<int>(type: "int", nullable: false),
                    FeeValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactionfee", x => new { x.TransactionReportingId, x.TransactionFeeReportingId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_transactionfee_FeeId",
                table: "transactionfee",
                column: "FeeId",
                unique: true);
        }
    }
}
