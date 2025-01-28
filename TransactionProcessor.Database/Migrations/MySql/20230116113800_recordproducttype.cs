#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class recordproducttype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductType",
                table: "contractproduct",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "contractproduct");
        }
    }
}
