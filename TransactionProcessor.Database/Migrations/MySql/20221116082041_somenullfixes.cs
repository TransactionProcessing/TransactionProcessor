#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    public partial class somenullfixes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RecipientMobile",
                table: "voucher",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientEmail",
                table: "voucher",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerAccountNumber",
                table: "transactionadditionalrequestdata",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Amount",
                table: "transactionadditionalrequestdata",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ActivityDescription",
                table: "statementline",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseMessage",
                table: "reconciliation",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseCode",
                table: "reconciliation",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceIdentifier",
                table: "reconciliation",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "voucher",
                keyColumn: "RecipientMobile",
                keyValue: null,
                column: "RecipientMobile",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientMobile",
                table: "voucher",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "voucher",
                keyColumn: "RecipientEmail",
                keyValue: null,
                column: "RecipientEmail",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientEmail",
                table: "voucher",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "transactionadditionalrequestdata",
                keyColumn: "CustomerAccountNumber",
                keyValue: null,
                column: "CustomerAccountNumber",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerAccountNumber",
                table: "transactionadditionalrequestdata",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "transactionadditionalrequestdata",
                keyColumn: "Amount",
                keyValue: null,
                column: "Amount",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Amount",
                table: "transactionadditionalrequestdata",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "statementline",
                keyColumn: "ActivityDescription",
                keyValue: null,
                column: "ActivityDescription",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ActivityDescription",
                table: "statementline",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "reconciliation",
                keyColumn: "ResponseMessage",
                keyValue: null,
                column: "ResponseMessage",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseMessage",
                table: "reconciliation",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "reconciliation",
                keyColumn: "ResponseCode",
                keyValue: null,
                column: "ResponseCode",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseCode",
                table: "reconciliation",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "reconciliation",
                keyColumn: "DeviceIdentifier",
                keyValue: null,
                column: "DeviceIdentifier",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "DeviceIdentifier",
                table: "reconciliation",
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
