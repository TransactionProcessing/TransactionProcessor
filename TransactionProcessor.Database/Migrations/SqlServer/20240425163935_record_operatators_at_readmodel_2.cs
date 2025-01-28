#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class record_operatators_at_readmodel_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator");

            migrationBuilder.DropColumn("OperatorReportingId", "estateoperator");

            migrationBuilder.AddColumn<int>(
                name: "OperatorReportingId",
                table: "estateoperator",
                type: "int",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator",
                columns: new[] { "EstateReportingId", "OperatorReportingId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator");

            migrationBuilder.AlterColumn<int>(
                name: "OperatorReportingId",
                table: "estateoperator",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator",
                column: "OperatorReportingId");
        }
    }
}
