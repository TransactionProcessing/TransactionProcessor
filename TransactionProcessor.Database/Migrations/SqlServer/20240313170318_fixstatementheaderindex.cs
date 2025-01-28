#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
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
                columns: new[] { "MerchantReportingId", "StatementGeneratedDate" })
                .Annotation("SqlServer:Clustered", true);
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
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
