#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class recordestateoperatorreportingidwithtxn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorIdentifier",
                table: "transaction");

            migrationBuilder.AddColumn<int>(
                name: "EstateOperatorReportingId",
                table: "transaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateOperatorReportingId",
                table: "estateoperator",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstateOperatorReportingId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "EstateOperatorReportingId",
                table: "estateoperator");

            migrationBuilder.AddColumn<string>(
                name: "OperatorIdentifier",
                table: "transaction",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
