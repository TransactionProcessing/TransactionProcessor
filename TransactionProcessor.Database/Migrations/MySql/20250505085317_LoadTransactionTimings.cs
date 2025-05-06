using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class LoadTransactionTimings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transactiontimings",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionStartedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OperatorCommunicationsStartedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OperatorCommunicationsCompletedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TransactionCompletedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalTransactionInMilliseconds = table.Column<double>(type: "double", nullable: false),
                    OperatorCommunicationsDurationInMilliseconds = table.Column<double>(type: "double", nullable: false),
                    TransactionProcessingDurationInMilliseconds = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactiontimings", x => x.TransactionId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactiontimings");
        }
    }
}
