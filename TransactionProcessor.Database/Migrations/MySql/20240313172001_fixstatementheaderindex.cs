#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class fixstatementheaderindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_statementheader_MerchantReportingId_StatementGeneratedDate",
                table: "statementheader");

            migrationBuilder.CreateIndex(
                name: "IX_statementheader_MerchantReportingId_StatementGeneratedDate",
                table: "statementheader",
                columns: new[] { "MerchantReportingId", "StatementGeneratedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_statementheader_MerchantReportingId_StatementGeneratedDate",
                table: "statementheader");

            migrationBuilder.CreateIndex(
                name: "IX_statementheader_MerchantReportingId_StatementGeneratedDate",
                table: "statementheader",
                columns: new[] { "MerchantReportingId", "StatementGeneratedDate" },
                unique: true);
        }
    }
}
