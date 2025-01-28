#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class addmerchantcontracttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MerchantContracts",
                columns: table => new
                {
                    MerchantReportingId = table.Column<int>(type: "int", nullable: false),
                    ContractReportingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantContracts", x => new { x.MerchantReportingId, x.ContractReportingId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantContracts");
        }
    }
}
