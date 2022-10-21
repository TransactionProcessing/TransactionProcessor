using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.ProjectionEngine.Migrations
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
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
