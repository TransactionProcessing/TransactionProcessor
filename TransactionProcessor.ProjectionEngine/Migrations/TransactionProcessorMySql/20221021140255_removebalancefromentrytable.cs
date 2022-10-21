using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.ProjectionEngine.Migrations.TransactionProcessorMySql
{
    public partial class removebalancefromentrytable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "MerchantBalanceChangedEntry");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "MerchantBalanceChangedEntry",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
