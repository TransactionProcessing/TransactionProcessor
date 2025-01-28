#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class record_operatators_at_readmodel_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_operator",
                table: "operator");

            migrationBuilder.AddPrimaryKey(
                name: "PK_operator",
                table: "operator",
                column: "OperatorReportingId");

            migrationBuilder.CreateIndex(
                name: "IX_operator_OperatorId",
                table: "operator",
                column: "OperatorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_operator",
                table: "operator");

            migrationBuilder.DropIndex(
                name: "IX_operator_OperatorId",
                table: "operator");

            migrationBuilder.AddPrimaryKey(
                name: "PK_operator",
                table: "operator",
                column: "OperatorId");
        }
    }
}
