#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
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
                    OperatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstateReportingId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequireCustomMerchantNumber = table.Column<bool>(type: "bit", nullable: false),
                    RequireCustomTerminalNumber = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operator", x => x.OperatorId);
                });
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
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "estateoperator",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RequireCustomMerchantNumber",
                table: "estateoperator",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCustomTerminalNumber",
                table: "estateoperator",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator",
                columns: new[] { "EstateReportingId", "OperatorId" });
        }
    }
}
