#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class task384_datamodelrefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "uvwFileImportLogView");

            migrationBuilder.DropTable(
                name: "uvwFileView");

            migrationBuilder.DropTable(
                name: "uvwTransactionsView");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionfee",
                table: "transactionfee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionadditionalresponsedata",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionadditionalrequestdata",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_statementline",
                table: "statementline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_statementheader",
                table: "statementheader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_settlement",
                table: "settlement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reconciliation",
                table: "reconciliation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantsettlementfee",
                table: "merchantsettlementfee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantsecurityuser",
                table: "merchantsecurityuser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantoperator",
                table: "merchantoperator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantdevice",
                table: "merchantdevice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantcontact",
                table: "merchantcontact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantaddress",
                table: "merchantaddress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchant",
                table: "merchant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fileline",
                table: "fileline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fileimportlogfile",
                table: "fileimportlogfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fileimportlog",
                table: "fileimportlog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_file",
                table: "file");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estatesecurityuser",
                table: "estatesecurityuser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estate",
                table: "estate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contractproducttransactionfee",
                table: "contractproducttransactionfee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contractproduct",
                table: "contractproduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contract",
                table: "contract");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "transactionfee");

            migrationBuilder.AddColumn<int>(
                                            name: "TransactionReportingId",
                                            table: "transactionadditionalresponsedata",
                                            type: "int",
                                            nullable: false,
                                            defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                                            name: "TransactionReportingId",
                                            table: "transactionadditionalrequestdata",
                                            type: "int",
                                            nullable: false,
                                            defaultValue: 0);

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "StatementId",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "statementheader");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "statementheader");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "settlement");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "reconciliation");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "reconciliation");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "SettlementId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "FeeId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "merchantsecurityuser");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantsecurityuser");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "merchantoperator");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantoperator");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "merchantdevice");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantdevice");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "merchantcontact");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantcontact");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "merchantaddress");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantaddress");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "fileline");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "fileline");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "fileline");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "FileImportLogId",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "fileimportlog");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "FileImportLogId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "estatesecurityuser");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "estateoperator");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "contractproducttransactionfee");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "contractproducttransactionfee");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "contractproducttransactionfee");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "contractproduct");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "contractproduct");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "contract");

            migrationBuilder.AlterColumn<string>(
                name: "VoucherCode",
                table: "voucher",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "voucher",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDateTime",
                table: "voucher",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "GenerateDate",
                table: "voucher",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedDate",
                table: "voucher",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RedeemedDate",
                table: "voucher",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "voucher",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "transactionfee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionFeeReportingId",
                table: "transactionfee",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDate",
                table: "transaction",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "transaction",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "ContractProductReportingId",
                table: "transaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractReportingId",
                table: "transaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "transaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatementReportingId",
                table: "statementline",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "statementline",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivityDate",
                table: "statementline",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatementGeneratedDate",
                table: "statementheader",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatementCreatedDate",
                table: "statementheader",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "statementheader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatementCreatedDateTime",
                table: "statementheader",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StatementGeneratedDateTime",
                table: "statementheader",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "StatementReportingId",
                table: "statementheader",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SettlementDate",
                table: "settlement",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<int>(
                name: "SettlementReportingId",
                table: "settlement",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "settlement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDate",
                table: "reconciliation",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "reconciliation",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "reconciliation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SettlementReportingId",
                table: "merchantsettlementfee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "merchantsettlementfee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionFeeReportingId",
                table: "merchantsettlementfee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "merchantsettlementfee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "merchantsecurityuser",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "merchantoperator",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "merchantdevice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "merchantcontact",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "merchantaddress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "merchant",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "merchant",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "FileReportingId",
                table: "fileline",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "fileline",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FileImportLogReportingId",
                table: "fileimportlogfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FileReportingId",
                table: "fileimportlogfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FileUploadedDate",
                table: "fileimportlogfile",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "fileimportlogfile",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "fileimportlog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FileImportLogReportingId",
                table: "fileimportlog",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportLogDate",
                table: "fileimportlog",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "FileReportingId",
                table: "file",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "file",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FileImportLogReportingId",
                table: "file",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FileReceivedDate",
                table: "file",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "file",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "estatesecurityuser",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "estateoperator",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "estate",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "ContractProductReportingId",
                table: "contractproducttransactionfee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionFeeReportingId",
                table: "contractproducttransactionfee",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "ContractReportingId",
                table: "contractproduct",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractProductReportingId",
                table: "contractproduct",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "contract",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractReportingId",
                table: "contract",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionfee",
                table: "transactionfee",
                columns: new[] { "TransactionReportingId", "TransactionFeeReportingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionadditionalresponsedata",
                table: "transactionadditionalresponsedata",
                column: "TransactionReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionadditionalrequestdata",
                table: "transactionadditionalrequestdata",
                column: "TransactionReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction",
                table: "transaction",
                column: "TransactionReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_statementline",
                table: "statementline",
                columns: new[] { "StatementReportingId", "TransactionReportingId", "ActivityDateTime", "ActivityType" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_statementheader",
                table: "statementheader",
                columns: new[] { "MerchantReportingId", "StatementId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_settlement",
                table: "settlement",
                column: "SettlementReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reconciliation",
                table: "reconciliation",
                column: "TransactionReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantsettlementfee",
                table: "merchantsettlementfee",
                columns: new[] { "SettlementReportingId", "TransactionReportingId", "TransactionFeeReportingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantsecurityuser",
                table: "merchantsecurityuser",
                columns: new[] { "MerchantReportingId", "SecurityUserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantoperator",
                table: "merchantoperator",
                columns: new[] { "MerchantReportingId", "OperatorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantdevice",
                table: "merchantdevice",
                columns: new[] { "MerchantReportingId", "DeviceId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantcontact",
                table: "merchantcontact",
                columns: new[] { "MerchantReportingId", "ContactId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantaddress",
                table: "merchantaddress",
                columns: new[] { "MerchantReportingId", "AddressId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchant",
                table: "merchant",
                columns: new[] { "EstateReportingId", "MerchantReportingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileline",
                table: "fileline",
                columns: new[] { "FileReportingId", "LineNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileimportlogfile",
                table: "fileimportlogfile",
                columns: new[] { "FileImportLogReportingId", "FileReportingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileimportlog",
                table: "fileimportlog",
                columns: new[] { "EstateReportingId", "FileImportLogReportingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_file",
                table: "file",
                column: "FileReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estatesecurityuser",
                table: "estatesecurityuser",
                columns: new[] { "SecurityUserId", "EstateReportingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator",
                columns: new[] { "EstateReportingId", "OperatorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_estate",
                table: "estate",
                column: "EstateReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_contractproducttransactionfee",
                table: "contractproducttransactionfee",
                columns: new[] { "ContractProductReportingId", "TransactionFeeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_contractproduct",
                table: "contractproduct",
                columns: new[] { "ContractReportingId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_contract",
                table: "contract",
                columns: new[] { "EstateReportingId", "OperatorId", "ContractId" });

            migrationBuilder.CreateIndex(
                name: "IX_voucher_TransactionReportingId",
                table: "voucher",
                column: "TransactionReportingId");

            migrationBuilder.CreateIndex(
                name: "IX_voucher_VoucherCode",
                table: "voucher",
                column: "VoucherCode");

            migrationBuilder.CreateIndex(
                name: "IX_transactionfee_FeeId",
                table: "transactionfee",
                column: "FeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transaction_TransactionDate_MerchantReportingId",
                table: "transaction",
                columns: new[] { "TransactionDate", "MerchantReportingId" });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_TransactionId_MerchantReportingId",
                table: "transaction",
                columns: new[] { "TransactionId", "MerchantReportingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_statementheader_MerchantReportingId_StatementGeneratedDate",
                table: "statementheader",
                columns: new[] { "MerchantReportingId", "StatementGeneratedDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlement_EstateReportingId_SettlementId",
                table: "settlement",
                columns: new[] { "EstateReportingId", "SettlementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlement_SettlementDate_EstateReportingId",
                table: "settlement",
                columns: new[] { "SettlementDate", "EstateReportingId" });

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_TransactionDate_MerchantReportingId",
                table: "reconciliation",
                columns: new[] { "TransactionDate", "MerchantReportingId" });

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_TransactionId_MerchantReportingId",
                table: "reconciliation",
                columns: new[] { "TransactionId", "MerchantReportingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_merchant_EstateReportingId_MerchantId",
                table: "merchant",
                columns: new[] { "EstateReportingId", "MerchantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fileline_TransactionReportingId",
                table: "fileline",
                column: "TransactionReportingId");

            migrationBuilder.CreateIndex(
                name: "IX_fileimportlog_EstateReportingId_FileImportLogId",
                table: "fileimportlog",
                columns: new[] { "EstateReportingId", "FileImportLogId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_file_FileId",
                table: "file",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estate_EstateId",
                table: "estate",
                column: "EstateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_voucher_TransactionReportingId",
                table: "voucher");

            migrationBuilder.DropIndex(
                name: "IX_voucher_VoucherCode",
                table: "voucher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionfee",
                table: "transactionfee");

            migrationBuilder.DropIndex(
                name: "IX_transactionfee_FeeId",
                table: "transactionfee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionadditionalresponsedata",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionadditionalrequestdata",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction",
                table: "transaction");

            migrationBuilder.DropIndex(
                name: "IX_transaction_TransactionDate_MerchantReportingId",
                table: "transaction");

            migrationBuilder.DropIndex(
                name: "IX_transaction_TransactionId_MerchantReportingId",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_statementline",
                table: "statementline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_statementheader",
                table: "statementheader");

            migrationBuilder.DropIndex(
                name: "IX_statementheader_MerchantReportingId_StatementGeneratedDate",
                table: "statementheader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_settlement",
                table: "settlement");

            migrationBuilder.DropIndex(
                name: "IX_settlement_EstateReportingId_SettlementId",
                table: "settlement");

            migrationBuilder.DropIndex(
                name: "IX_settlement_SettlementDate_EstateReportingId",
                table: "settlement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reconciliation",
                table: "reconciliation");

            migrationBuilder.DropIndex(
                name: "IX_reconciliation_TransactionDate_MerchantReportingId",
                table: "reconciliation");

            migrationBuilder.DropIndex(
                name: "IX_reconciliation_TransactionId_MerchantReportingId",
                table: "reconciliation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantsettlementfee",
                table: "merchantsettlementfee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantsecurityuser",
                table: "merchantsecurityuser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantoperator",
                table: "merchantoperator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantdevice",
                table: "merchantdevice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantcontact",
                table: "merchantcontact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantaddress",
                table: "merchantaddress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchant",
                table: "merchant");

            migrationBuilder.DropIndex(
                name: "IX_merchant_EstateReportingId_MerchantId",
                table: "merchant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fileline",
                table: "fileline");

            migrationBuilder.DropIndex(
                name: "IX_fileline_TransactionReportingId",
                table: "fileline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fileimportlogfile",
                table: "fileimportlogfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fileimportlog",
                table: "fileimportlog");

            migrationBuilder.DropIndex(
                name: "IX_fileimportlog_EstateReportingId_FileImportLogId",
                table: "fileimportlog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_file",
                table: "file");

            migrationBuilder.DropIndex(
                name: "IX_file_FileId",
                table: "file");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estatesecurityuser",
                table: "estatesecurityuser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estate",
                table: "estate");

            migrationBuilder.DropIndex(
                name: "IX_estate_EstateId",
                table: "estate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contractproducttransactionfee",
                table: "contractproducttransactionfee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contractproduct",
                table: "contractproduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contract",
                table: "contract");

            migrationBuilder.DropColumn(
                name: "ExpiryDateTime",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "GenerateDate",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "IssuedDate",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "RedeemedDate",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "transactionfee");

            migrationBuilder.DropColumn(
                name: "TransactionFeeReportingId",
                table: "transactionfee");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "ContractProductReportingId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "ContractReportingId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "StatementReportingId",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "ActivityDate",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "statementheader");

            migrationBuilder.DropColumn(
                name: "StatementCreatedDateTime",
                table: "statementheader");

            migrationBuilder.DropColumn(
                name: "StatementGeneratedDateTime",
                table: "statementheader");

            migrationBuilder.DropColumn(
                name: "StatementReportingId",
                table: "statementheader");

            migrationBuilder.DropColumn(
                name: "SettlementReportingId",
                table: "settlement");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "settlement");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "reconciliation");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "reconciliation");

            migrationBuilder.DropColumn(
                name: "SettlementReportingId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "TransactionFeeReportingId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "merchantsecurityuser");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "merchantoperator");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "merchantdevice");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "merchantcontact");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "merchantaddress");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "FileReportingId",
                table: "fileline");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "fileline");

            migrationBuilder.DropColumn(
                name: "FileImportLogReportingId",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "FileReportingId",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "FileUploadedDate",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "fileimportlog");

            migrationBuilder.DropColumn(
                name: "FileImportLogReportingId",
                table: "fileimportlog");

            migrationBuilder.DropColumn(
                name: "ImportLogDate",
                table: "fileimportlog");

            migrationBuilder.DropColumn(
                name: "FileReportingId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "FileImportLogReportingId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "FileReceivedDate",
                table: "file");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "estatesecurityuser");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "estateoperator");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "estate");

            migrationBuilder.DropColumn(
                name: "ContractProductReportingId",
                table: "contractproducttransactionfee");

            migrationBuilder.DropColumn(
                name: "TransactionFeeReportingId",
                table: "contractproducttransactionfee");

            migrationBuilder.DropColumn(
                name: "ContractReportingId",
                table: "contractproduct");

            migrationBuilder.DropColumn(
                name: "ContractProductReportingId",
                table: "contractproduct");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "contract");

            migrationBuilder.DropColumn(
                name: "ContractReportingId",
                table: "contract");

            migrationBuilder.AlterColumn<string>(
                name: "VoucherCode",
                table: "voucher",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "voucher",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "voucher",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "voucher",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "transactionfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "transactionadditionalresponsedata",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "transactionadditionalresponsedata",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "transactionadditionalresponsedata",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "transactionadditionalrequestdata",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "transactionadditionalrequestdata",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "transactionadditionalrequestdata",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDate",
                table: "transaction",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "transaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "transaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "transaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "transaction",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "StatementId",
                table: "statementline",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "statementline",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "statementline",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "statementline",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatementGeneratedDate",
                table: "statementheader",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StatementCreatedDate",
                table: "statementheader",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "statementheader",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "statementheader",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SettlementDate",
                table: "settlement",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "settlement",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDate",
                table: "reconciliation",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "reconciliation",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "reconciliation",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "merchantsettlementfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "SettlementId",
                table: "merchantsettlementfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "merchantsettlementfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "FeeId",
                table: "merchantsettlementfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "merchantsettlementfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "merchantsecurityuser",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "merchantsecurityuser",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "merchantoperator",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "merchantoperator",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "merchantdevice",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "merchantdevice",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "merchantcontact",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "merchantcontact",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "merchantaddress",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "merchantaddress",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "merchant",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "fileline",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "fileline",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "fileline",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "fileimportlogfile",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "FileImportLogId",
                table: "fileimportlogfile",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "fileimportlogfile",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "fileimportlogfile",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "fileimportlog",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "file",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "FileImportLogId",
                table: "file",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "file",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "estatesecurityuser",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "estateoperator",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "contractproducttransactionfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "contractproducttransactionfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "contractproducttransactionfee",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "contractproduct",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "contractproduct",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "contract",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionfee",
                table: "transactionfee",
                columns: new[] { "TransactionId", "FeeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionadditionalresponsedata",
                table: "transactionadditionalresponsedata",
                column: "TransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionadditionalrequestdata",
                table: "transactionadditionalrequestdata",
                columns: new[] { "EstateId", "MerchantId", "TransactionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction",
                table: "transaction",
                columns: new[] { "EstateId", "MerchantId", "TransactionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_statementline",
                table: "statementline",
                columns: new[] { "StatementId", "TransactionId", "ActivityDateTime", "ActivityType" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_statementheader",
                table: "statementheader",
                column: "StatementId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_settlement",
                table: "settlement",
                columns: new[] { "EstateId", "SettlementId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_reconciliation",
                table: "reconciliation",
                column: "TransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantsettlementfee",
                table: "merchantsettlementfee",
                columns: new[] { "EstateId", "SettlementId", "TransactionId", "FeeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantsecurityuser",
                table: "merchantsecurityuser",
                columns: new[] { "EstateId", "MerchantId", "SecurityUserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantoperator",
                table: "merchantoperator",
                columns: new[] { "EstateId", "MerchantId", "OperatorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantdevice",
                table: "merchantdevice",
                columns: new[] { "EstateId", "MerchantId", "DeviceId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantcontact",
                table: "merchantcontact",
                columns: new[] { "EstateId", "MerchantId", "ContactId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantaddress",
                table: "merchantaddress",
                columns: new[] { "EstateId", "MerchantId", "AddressId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchant",
                table: "merchant",
                columns: new[] { "EstateId", "MerchantId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileline",
                table: "fileline",
                columns: new[] { "EstateId", "FileId", "LineNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileimportlogfile",
                table: "fileimportlogfile",
                columns: new[] { "EstateId", "FileImportLogId", "FileId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileimportlog",
                table: "fileimportlog",
                columns: new[] { "EstateId", "FileImportLogId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_file",
                table: "file",
                columns: new[] { "EstateId", "FileImportLogId", "FileId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_estatesecurityuser",
                table: "estatesecurityuser",
                columns: new[] { "SecurityUserId", "EstateId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_estateoperator",
                table: "estateoperator",
                columns: new[] { "EstateId", "OperatorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_estate",
                table: "estate",
                column: "EstateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_contractproducttransactionfee",
                table: "contractproducttransactionfee",
                columns: new[] { "EstateId", "ContractId", "ProductId", "TransactionFeeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_contractproduct",
                table: "contractproduct",
                columns: new[] { "EstateId", "ContractId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_contract",
                table: "contract",
                columns: new[] { "EstateId", "OperatorId", "ContractId" });

            migrationBuilder.CreateTable(
                name: "uvwFileImportLogView",
                columns: table => new
                {
                    FileCount = table.Column<int>(type: "int", nullable: false),
                    FileImportLogId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ImportLogDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ImportLogDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ImportLogTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "uvwFileView",
                columns: table => new
                {
                    EmailAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    FileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FileReceivedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FileReceivedDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FileReceivedTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LineCount = table.Column<int>(type: "int", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MerchantName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PendingCount = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "uvwTransactionsView",
                columns: table => new
                {
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DayOfWeek = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsAuthorised = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MerchantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Month = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MonthNumber = table.Column<int>(type: "int", nullable: false),
                    OperatorIdentifier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    YearNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
