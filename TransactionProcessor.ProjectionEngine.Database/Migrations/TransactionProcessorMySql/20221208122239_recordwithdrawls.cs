#nullable disable

namespace TransactionProcessor.ProjectionEngine.Database.Migrations.TransactionProcessorMySql
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class recordwithdrawls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastWithdrawal",
                table: "MerchantBalanceProjectionState",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWithdrawn",
                table: "MerchantBalanceProjectionState",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WithdrawalCount",
                table: "MerchantBalanceProjectionState",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWithdrawal",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "TotalWithdrawn",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "WithdrawalCount",
                table: "MerchantBalanceProjectionState");
        }
    }
}
