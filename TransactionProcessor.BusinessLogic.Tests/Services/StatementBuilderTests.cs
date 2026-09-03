using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services {
    public class StatementBuilderTests {
        private readonly MockFileSystem _fileSystem;
        private readonly StatementBuilder _builder;
        private readonly MerchantStatementAggregate merchantStatementAggregate;
        private readonly Merchant _merchant;
        private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

        public StatementBuilderTests() {
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
            _fileSystem = new MockFileSystem();
            _builder = new StatementBuilder(_fileSystem);

            // Setup minimal merchant and statement aggregate
            _merchant = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts(SettlementSchedule.Immediate);

            merchantStatementAggregate = new MerchantStatementAggregate();
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId1, new DateTime(2025, 5, 1));
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId2, new DateTime(2025, 5, 2));
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 1), new MerchantStatementSummaryTotals(100, 1000.00m, 100, 10.00m, 1, 1000, 1, 200));
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 2), new MerchantStatementSummaryTotals(200, 2000.00m, 200, 20.00m, 2, 1000, 2, 200));
            merchantStatementAggregate.GenerateStatement(TestData.GeneratedDateTime);

            // Seed the templates and CSS consumed by StatementBuilder.
            String templateDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Templates", "Email");
            _fileSystem.AddFile(Path.Combine(templateDirectory, "statement.html"), new MockFileData(
                "Transactions Fees [MerchantName] [MerchantAddressLine1] [StatementLinesData] {bootstrapcss} {fontawesomemincss} {fontawesomesolidcss}"));
            _fileSystem.AddFile(Path.Combine(templateDirectory, "statementline.html"), new MockFileData(
                "[StatementLineDate] [StatementLineAmount]"));
            String cssDirectory = Path.Combine(AppContext.BaseDirectory, "Templates", "Email");
            _fileSystem.AddFile(Path.Combine(cssDirectory, "bootstrap", "css", "bootstrap.min.css"), new MockFileData("body{color:red;}"));
            _fileSystem.AddFile(Path.Combine(cssDirectory, "fontawesome", "css", "fontawesome.min.css"), new MockFileData(".fa{display:inline;}"));
            _fileSystem.AddFile(Path.Combine(cssDirectory, "fontawesome", "css", "solid.css"), new MockFileData(".fa-solid{font-weight:bold;}"));
        }

        [Fact]
        public async Task GetStatementHtml_ReturnsHtmlWithReplacedTokens() {
            // Arrange
            // (Mocks already set up in constructor)

            // Act
            Result<String> htmlResult = await _builder.GetStatementHtml(merchantStatementAggregate, _merchant, _cancellationToken);

            htmlResult.IsSuccess.ShouldBeTrue();
            String html = htmlResult.Data;
            // Assert
            html.ShouldContain(this._merchant.MerchantName);
            html.ShouldContain(this._merchant.Addresses.First().AddressLine1);
            html.ShouldContain("Transactions");
            html.ShouldContain("Fees");
            html.ShouldContain("01/05/2025");
            html.ShouldContain("02/05/2025");
            html.ShouldContain("100");
            html.ShouldContain("10");
            html.ShouldContain("body{color:red;}");
            html.ShouldContain(".fa{display:inline;}");
            html.ShouldContain(".fa-solid{font-weight:bold;}");
        }

        [Fact]
        public async Task GetStatementHtml_StatementNotGenerated_ErrorResult()
        {
            // Arrange
            var merchantStatementAggregate = new MerchantStatementAggregate();
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId1, new DateTime(2025, 5, 1));
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId2, new DateTime(2025, 5, 2));
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 1), new MerchantStatementSummaryTotals(100, 1000.00m, 100, 10.00m, 1, 1000, 1, 200));
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 2), new MerchantStatementSummaryTotals(200, 2000.00m, 200, 20.00m, 2, 1000, 2, 200));
            
            // Act
            Result<String> htmlResult = await _builder.GetStatementHtml(merchantStatementAggregate, _merchant, _cancellationToken);

            htmlResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task GetStatementHtml_StatementAleadyBuilt_ErrorResult()
        {
            // Arrange
            var merchantStatementAggregate = new MerchantStatementAggregate();
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId1, new DateTime(2025, 5, 1));
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId2, new DateTime(2025, 5, 2));
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 1), new MerchantStatementSummaryTotals(100, 1000.00m, 100, 10.00m, 1, 1000, 1, 200));
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 2), new MerchantStatementSummaryTotals(200, 2000.00m, 200, 20.00m, 2, 1000, 2, 200));
            merchantStatementAggregate.GenerateStatement(TestData.GeneratedDateTime);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, "<html>statement</html>");
            // Act
            Result<String> htmlResult = await _builder.GetStatementHtml(merchantStatementAggregate, _merchant, _cancellationToken);

            htmlResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task GetStatementHtml_ThrowsIfTemplateMissing() {
            // Arrange
            String templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Templates", "Email", "statement.html");
            _fileSystem.File.Delete(templatePath);

            // Act & Assert
            await Should.ThrowAsync<FileNotFoundException>(async () => { await _builder.GetStatementHtml(merchantStatementAggregate, _merchant, _cancellationToken); });
        }

        [Fact]
        public async Task GetStatementHtml_ThrowsIfMerchantAddressMissing() {
            // Arrange
            var merchant = new Merchant {
                MerchantName = TestData.Merchant.MerchantName,
                Addresses = new List<Address>(), // No address
                Contacts = _merchant.Contacts
            };

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () => { await _builder.GetStatementHtml(merchantStatementAggregate, merchant, _cancellationToken); });
        }

        [Fact]
        public async Task GetStatementHtml_ThrowsIfMerchantContactMissing() {
            // Arrange
            var merchant = new Merchant {
                MerchantName = TestData.Merchant.MerchantName, Addresses = _merchant.Addresses, Contacts = new List<Contact>() // No contact
            };

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(async () => { await _builder.GetStatementHtml(merchantStatementAggregate, merchant, _cancellationToken); });
        }
    }
}

