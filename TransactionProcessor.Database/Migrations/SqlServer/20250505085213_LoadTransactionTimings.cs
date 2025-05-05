using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.Database.Migrations.SqlServer
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
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionStartedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperatorCommunicationsStartedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperatorCommunicationsCompletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionCompletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalTransactionInMilliseconds = table.Column<double>(type: "float", nullable: false),
                    OperatorCommunicationsDurationInMilliseconds = table.Column<double>(type: "float", nullable: false),
                    TransactionProcessingDurationInMilliseconds = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactiontimings", x => x.TransactionId)
                        .Annotation("SqlServer:Clustered", false);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactiontimings");
        }
    }
}
