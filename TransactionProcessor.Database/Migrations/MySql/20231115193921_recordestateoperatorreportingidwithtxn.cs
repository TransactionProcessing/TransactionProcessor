#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
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
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
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
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
