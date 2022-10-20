using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.ProjectionEngine.Migrations.TransactionProcessorMySql
{
    public partial class recordmoreinfoatstate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AuthorisedSales",
                table: "MerchantBalanceProjectionState",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CompletedTransactionCount",
                table: "MerchantBalanceProjectionState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DeclinedSales",
                table: "MerchantBalanceProjectionState",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DepositCount",
                table: "MerchantBalanceProjectionState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FeeCount",
                table: "MerchantBalanceProjectionState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDeposit",
                table: "MerchantBalanceProjectionState",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFee",
                table: "MerchantBalanceProjectionState",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSale",
                table: "MerchantBalanceProjectionState",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SaleCount",
                table: "MerchantBalanceProjectionState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartedTransactionCount",
                table: "MerchantBalanceProjectionState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDeposited",
                table: "MerchantBalanceProjectionState",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValueOfFees",
                table: "MerchantBalanceProjectionState",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorisedSales",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "CompletedTransactionCount",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "DeclinedSales",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "DepositCount",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "FeeCount",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "LastDeposit",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "LastFee",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "LastSale",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "SaleCount",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "StartedTransactionCount",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "TotalDeposited",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "ValueOfFees",
                table: "MerchantBalanceProjectionState");
        }
    }
}
