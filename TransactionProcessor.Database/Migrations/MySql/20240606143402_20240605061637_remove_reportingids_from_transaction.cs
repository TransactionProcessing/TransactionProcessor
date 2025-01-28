#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace TransactionProcessor.Database.Migrations.MySql
{
    /// <inheritdoc />
    public partial class _20240605061637_remove_reportingids_from_transaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "estateoperator");

            migrationBuilder.DropIndex(
                name: "IX_voucher_TransactionReportingId",
                table: "voucher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionadditionalresponsedata",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionadditionalrequestdata",
                table: "transactionadditionalrequestdata");

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

            migrationBuilder.DropIndex(
                name: "IX_settlement_EstateReportingId_SettlementId",
                table: "settlement");

            migrationBuilder.DropIndex(
                name: "IX_settlement_SettlementDate_EstateReportingId",
                table: "settlement");

            migrationBuilder.DropIndex(
                name: "IX_reconciliation_TransactionDate_MerchantReportingId",
                table: "reconciliation");

            migrationBuilder.DropIndex(
                name: "IX_reconciliation_TransactionId_MerchantReportingId",
                table: "reconciliation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_merchantsettlementfee",
                table: "merchantsettlementfee");

            migrationBuilder.DropIndex(
                name: "IX_merchantsettlementfee_TransactionReportingId",
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
                name: "PK_MerchantContracts",
                table: "MerchantContracts");

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
                name: "PK_estatesecurityuser",
                table: "estatesecurityuser");

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
                name: "TransactionReportingId",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "TransactionReportingId",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropColumn(
                name: "ContractProductReportingId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "ContractReportingId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "EstateOperatorReportingId",
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
                name: "MerchantReportingId",
                table: "statementheader");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "settlement");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "settlement");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "reconciliation");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "operator");

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
                table: "MerchantContracts");

            migrationBuilder.DropColumn(
                name: "ContractReportingId",
                table: "MerchantContracts");

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
                name: "MerchantReportingId",
                table: "fileimportlogfile");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "fileimportlog");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "FileImportLogReportingId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "MerchantReportingId",
                table: "file");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "estatesecurityuser");

            migrationBuilder.DropColumn(
                name: "ContractProductReportingId",
                table: "contractproducttransactionfee");

            migrationBuilder.DropColumn(
                name: "ContractReportingId",
                table: "contractproduct");

            migrationBuilder.DropColumn(
                name: "EstateReportingId",
                table: "contract");

            migrationBuilder.RenameColumn(
                name: "TransactionFeeReportingId",
                table: "contractproducttransactionfee",
                newName: "ContractProductTransactionFeeReportingId");

            migrationBuilder.RenameColumn(
                name: "TransactionFeeId",
                table: "contractproducttransactionfee",
                newName: "ContractProductTransactionFeeId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "contractproduct",
                newName: "ContractProductId");

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "voucher",
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
                name: "TransactionId",
                table: "transactionadditionalrequestdata",
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
                name: "ContractProductId",
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
                name: "OperatorId",
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
                name: "MerchantId",
                table: "statementheader",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "EstateId",
                table: "settlement",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "settlement",
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
                table: "operator",
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
                name: "ContractProductTransactionFeeId",
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
                name: "MerchantId",
                table: "merchantsecurityuser",
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
                name: "MerchantId",
                table: "merchantdevice",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId",
                table: "MerchantContracts",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "MerchantContracts",
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
                name: "ContractProductId",
                table: "contractproducttransactionfee",
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
                name: "PK_transactionadditionalresponsedata",
                table: "transactionadditionalresponsedata",
                column: "TransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionadditionalrequestdata",
                table: "transactionadditionalrequestdata",
                column: "TransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_statementline",
                table: "statementline",
                columns: new[] { "StatementId", "TransactionId", "ActivityDateTime", "ActivityType" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_statementheader",
                table: "statementheader",
                columns: new[] { "MerchantId", "StatementId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantsettlementfee",
                table: "merchantsettlementfee",
                columns: new[] { "SettlementId", "TransactionId", "ContractProductTransactionFeeId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantsecurityuser",
                table: "merchantsecurityuser",
                columns: new[] { "MerchantId", "SecurityUserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantoperator",
                table: "merchantoperator",
                columns: new[] { "MerchantId", "OperatorId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantdevice",
                table: "merchantdevice",
                columns: new[] { "MerchantId", "DeviceId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_MerchantContracts",
                table: "MerchantContracts",
                columns: new[] { "MerchantId", "ContractId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantcontact",
                table: "merchantcontact",
                columns: new[] { "MerchantId", "ContactId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchantaddress",
                table: "merchantaddress",
                columns: new[] { "MerchantId", "AddressId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_merchant",
                table: "merchant",
                column: "MerchantReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileline",
                table: "fileline",
                columns: new[] { "FileId", "LineNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileimportlogfile",
                table: "fileimportlogfile",
                columns: new[] { "FileImportLogId", "FileId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_fileimportlog",
                table: "fileimportlog",
                columns: new[] { "EstateId", "FileImportLogReportingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_estatesecurityuser",
                table: "estatesecurityuser",
                columns: new[] { "SecurityUserId", "EstateId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_contractproducttransactionfee",
                table: "contractproducttransactionfee",
                column: "ContractProductTransactionFeeReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_contractproduct",
                table: "contractproduct",
                column: "ContractProductReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_contract",
                table: "contract",
                columns: new[] { "EstateId", "OperatorId", "ContractId" });

            migrationBuilder.CreateIndex(
                name: "IX_voucher_TransactionId",
                table: "voucher",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_TransactionDate",
                table: "transaction",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_statementheader_StatementGeneratedDate",
                table: "statementheader",
                column: "StatementGeneratedDate");

            migrationBuilder.CreateIndex(
                name: "IX_settlement_EstateId_SettlementId",
                table: "settlement",
                columns: new[] { "EstateId", "SettlementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlement_SettlementDate",
                table: "settlement",
                column: "SettlementDate");

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_TransactionDate",
                table: "reconciliation",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_TransactionId_MerchantId",
                table: "reconciliation",
                columns: new[] { "TransactionId", "MerchantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_merchant_EstateId_MerchantId",
                table: "merchant",
                columns: new[] { "EstateId", "MerchantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fileimportlog_EstateId_FileImportLogId",
                table: "fileimportlog",
                columns: new[] { "EstateId", "FileImportLogId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contractproducttransactionfee_ContractProductTransactionFeeI~",
                table: "contractproducttransactionfee",
                columns: new[] { "ContractProductTransactionFeeId", "ContractProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contractproduct_ContractProductId_ContractId",
                table: "contractproduct",
                columns: new[] { "ContractProductId", "ContractId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_voucher_TransactionId",
                table: "voucher");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionadditionalresponsedata",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transactionadditionalrequestdata",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropIndex(
                name: "IX_transaction_TransactionDate",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_statementline",
                table: "statementline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_statementheader",
                table: "statementheader");

            migrationBuilder.DropIndex(
                name: "IX_statementheader_StatementGeneratedDate",
                table: "statementheader");

            migrationBuilder.DropIndex(
                name: "IX_settlement_EstateId_SettlementId",
                table: "settlement");

            migrationBuilder.DropIndex(
                name: "IX_settlement_SettlementDate",
                table: "settlement");

            migrationBuilder.DropIndex(
                name: "IX_reconciliation_TransactionDate",
                table: "reconciliation");

            migrationBuilder.DropIndex(
                name: "IX_reconciliation_TransactionId_MerchantId",
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
                name: "PK_MerchantContracts",
                table: "MerchantContracts");

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
                name: "IX_merchant_EstateId_MerchantId",
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

            migrationBuilder.DropIndex(
                name: "IX_fileimportlog_EstateId_FileImportLogId",
                table: "fileimportlog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estatesecurityuser",
                table: "estatesecurityuser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contractproducttransactionfee",
                table: "contractproducttransactionfee");

            migrationBuilder.DropIndex(
                name: "IX_contractproducttransactionfee_ContractProductTransactionFeeI~",
                table: "contractproducttransactionfee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contractproduct",
                table: "contractproduct");

            migrationBuilder.DropIndex(
                name: "IX_contractproduct_ContractProductId_ContractId",
                table: "contractproduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_contract",
                table: "contract");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "voucher");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "transactionadditionalresponsedata");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "transactionadditionalrequestdata");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "ContractProductId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "StatementId",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "statementline");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "statementheader");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "settlement");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "settlement");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "reconciliation");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "operator");

            migrationBuilder.DropColumn(
                name: "SettlementId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "ContractProductTransactionFeeId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantsettlementfee");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantsecurityuser");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantoperator");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantdevice");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "MerchantContracts");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "MerchantContracts");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantcontact");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "merchantaddress");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "merchant");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "fileline");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "fileline");

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
                name: "ContractProductId",
                table: "contractproducttransactionfee");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "contractproduct");

            migrationBuilder.DropColumn(
                name: "EstateId",
                table: "contract");

            migrationBuilder.RenameColumn(
                name: "ContractProductTransactionFeeId",
                table: "contractproducttransactionfee",
                newName: "TransactionFeeId");

            migrationBuilder.RenameColumn(
                name: "ContractProductTransactionFeeReportingId",
                table: "contractproducttransactionfee",
                newName: "TransactionFeeReportingId");

            migrationBuilder.RenameColumn(
                name: "ContractProductId",
                table: "contractproduct",
                newName: "ProductId");

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "voucher",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransactionReportingId",
                table: "transactionadditionalrequestdata",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                name: "EstateOperatorReportingId",
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

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "statementheader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "settlement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "settlement",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MerchantReportingId",
                table: "reconciliation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "operator",
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
                table: "MerchantContracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractReportingId",
                table: "MerchantContracts",
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
                name: "ContractProductReportingId",
                table: "contractproducttransactionfee",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContractReportingId",
                table: "contractproduct",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstateReportingId",
                table: "contract",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionadditionalresponsedata",
                table: "transactionadditionalresponsedata",
                column: "TransactionReportingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactionadditionalrequestdata",
                table: "transactionadditionalrequestdata",
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
                name: "PK_MerchantContracts",
                table: "MerchantContracts",
                columns: new[] { "MerchantReportingId", "ContractReportingId" });

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
                name: "PK_estatesecurityuser",
                table: "estatesecurityuser",
                columns: new[] { "SecurityUserId", "EstateReportingId" });

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

            migrationBuilder.CreateTable(
                name: "estateoperator",
                columns: table => new
                {
                    EstateReportingId = table.Column<int>(type: "int", nullable: false),
                    OperatorReportingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estateoperator", x => new { x.EstateReportingId, x.OperatorReportingId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_voucher_TransactionReportingId",
                table: "voucher",
                column: "TransactionReportingId");

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
                columns: new[] { "MerchantReportingId", "StatementGeneratedDate" });

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
                name: "IX_merchantsettlementfee_TransactionReportingId",
                table: "merchantsettlementfee",
                column: "TransactionReportingId");

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
        }
    }
}
