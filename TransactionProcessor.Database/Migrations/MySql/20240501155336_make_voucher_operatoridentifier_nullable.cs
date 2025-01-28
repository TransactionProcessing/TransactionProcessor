#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class make_voucher_operatoridentifier_nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OperatorIdentifier",
                table: "voucher",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "voucher",
                keyColumn: "OperatorIdentifier",
                keyValue: null,
                column: "OperatorIdentifier",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "OperatorIdentifier",
                table: "voucher",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
