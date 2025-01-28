#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class record_operatators_at_readmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "estateoperator");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "estateoperator");

            migrationBuilder.DropColumn(
                name: "RequireCustomMerchantNumber",
                table: "estateoperator");

            migrationBuilder.DropColumn(
                name: "RequireCustomTerminalNumber",
                table: "estateoperator");

            migrationBuilder.RenameColumn(
                name: "EstateOperatorReportingId",
                table: "estateoperator",
                newName: "OperatorReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator",
                column: "OperatorReportingId");

            migrationBuilder.CreateTable(
                name: "operator",
                columns: table => new
                {
                    OperatorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EstateReportingId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequireCustomMerchantNumber = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequireCustomTerminalNumber = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operator", x => x.OperatorId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator");

            migrationBuilder.RenameColumn(
                name: "OperatorReportingId",
                table: "estateoperator",
                newName: "EstateOperatorReportingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OperatorId",
                table: "estateoperator",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "estateoperator",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "RequireCustomMerchantNumber",
                table: "estateoperator",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCustomTerminalNumber",
                table: "estateoperator",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator",
                columns: new[] { "EstateReportingId", "OperatorId" });
        }
    }
}
