using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransactionProcessor.ProjectionEngine.Migrations
{
    /// <inheritdoc />
    public partial class recordwithdrawls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Events",
                table: "Events");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWithdrawal",
                table: "MerchantBalanceProjectionState",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWithdrawn",
                table: "MerchantBalanceProjectionState",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WithdrawalCount",
                table: "MerchantBalanceProjectionState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                table: "Events",
                columns: new[] { "EventId", "Type" })
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Events",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LastWithdrawal",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "TotalWithdrawn",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.DropColumn(
                name: "WithdrawalCount",
                table: "MerchantBalanceProjectionState");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Events",
                table: "Events",
                columns: new[] { "EventId", "Type" })
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
