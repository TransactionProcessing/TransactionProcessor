#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class amountintransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TransactionAmount",
                table: "transaction",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_transaction_TransactionId",
                table: "transaction",
                column: "TransactionId",
                unique: true)
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transaction_TransactionId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "TransactionAmount",
                table: "transaction");
        }
    }
}
