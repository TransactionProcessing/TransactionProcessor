using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Services {
    public class StatementBuilderTests {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly StatementBuilder _builder;
        private readonly MerchantStatementAggregate merchantStatementAggregate;
        private readonly Merchant _merchant;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public StatementBuilderTests() {
            _fileSystemMock = new Mock<IFileSystem>();
            _builder = new StatementBuilder(_fileSystemMock.Object);

            // Setup minimal merchant and statement aggregate
            _merchant = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts(SettlementSchedule.Immediate);

            merchantStatementAggregate = new MerchantStatementAggregate();
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId1, new DateTime(2025, 5, 1));
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId2, new DateTime(2025, 5, 2));
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 1), 100, 1000.00m, 100, 10.00m, 1, 1000, 1, 200);
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 2), 200, 2000.00m, 200, 20.00m, 2, 1000, 2, 200);
            merchantStatementAggregate.GenerateStatement(TestData.GeneratedDateTime);

            // Setup file system mocks for templates and CSS
            var dirMock = new Mock<IDirectoryInfo>();
            _fileSystemMock.Setup(fs => fs.Directory.GetParent(It.IsAny<string>())).Returns(dirMock.Object);
            _fileSystemMock.SetupSequence(f => f.File.ReadAllTextAsync(It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync("<style>{bootstrapcss}</style>\r\n<style>{fontawesomemincss}</style>\r\n<style>{fontawesomesolidcss}</style>\r\n<style>\r\n    {\r\n        bootstrap .css\r\n    }</style>\r\n    <style>\r\n        body {\r\n            color: #484b51;\r\n            margin-top: 20px;\r\n        }\r\n\r\n        .container {\r\n            max-width: 960px;\r\n        }\r\n\r\n        .text-secondary-d1 { color: #728299 !important; }\r\n\r\n        .page-header {\r\n            -ms-flex-align: center;\r\n            -ms-flex-pack: justify;\r\n            align-items: center;\r\n            border-bottom: 1px dotted #e2e2e2;\r\n            display: -ms-flexbox;\r\n            display: flex;\r\n            justify-content: space-between;\r\n            margin: 0 0 1rem;\r\n            padding-bottom: 1rem;\r\n            padding-top: .5rem;\r\n        }\r\n\r\n        .page-title {\r\n            font-size: 1.75rem;\r\n            font-weight: 300;\r\n            margin: 0;\r\n            padding: 0;\r\n        }\r\n\r\n        .brc-default-l1 { border-color: #dce9f0 !important; }\r\n\r\n        .ml-n1, .mx-n1 { margin-left: -.25rem !important; }\r\n\r\n        .mr-n1, .mx-n1 { margin-right: -.25rem !important; }\r\n\r\n        .mb-4, .my-4 { margin-bottom: 1.5rem !important; }\r\n\r\n        hr {\r\n            border: 0;\r\n            border-top: 1px solid rgba(0, 0, 0, .1);\r\n            margin-bottom: 1rem;\r\n            margin-top: 1rem;\r\n        }\r\n\r\n        .text-grey-m2 { color: #888a8d !important; }\r\n\r\n        .text-success-m2 { color: #86bd68 !important; }\r\n\r\n        .font-bolder, .text-600 { font-weight: 600 !important; }\r\n\r\n        .text-110 { font-size: 110% !important; }\r\n\r\n        .text-blue { color: #478fcc !important; }\r\n\r\n        .pb-25, .py-25 { padding-bottom: .75rem !important; }\r\n\r\n        .pt-25, .py-25 { padding-top: .75rem !important; }\r\n\r\n        .bgc-default-tp1 { background-color: rgba(121, 169, 197, .92) !important; }\r\n\r\n        .bgc-default-l4, .bgc-h-default-l4:hover { background-color: #f3f8fa !important; }\r\n\r\n        .page-header .page-tools {\r\n            -ms-flex-item-align: end;\r\n            align-self: flex-end;\r\n        }\r\n\r\n        .btn-light {\r\n            background-color: #f5f6f9;\r\n            border-color: #dddfe4;\r\n            color: #757984;\r\n        }\r\n\r\n        .w-2 { width: 1rem; }\r\n\r\n        .text-120 { font-size: 120% !important; }\r\n\r\n        .text-primary-m1 { color: #4087d4 !important; }\r\n\r\n        .text-danger-m1 { color: #dd4949 !important; }\r\n\r\n        .text-blue-m2 { color: #68a3d5 !important; }\r\n\r\n        .text-150 { font-size: 150% !important; }\r\n\r\n        .text-60 { font-size: 60% !important; }\r\n\r\n        .text-grey-m1 { color: #7b7d81 !important; }\r\n\r\n        .align-bottom { vertical-align: bottom !important; }\r\n    </style>\r\n    <body style=\"width: 100%; border: 0px solid green\">\r\n        <div class=\"container\" style=\"border: 0px solid red\">\r\n            <div class=\"page-header text-blue-d2\">\r\n                <h1 class=\"page-title text-secondary-d1\">\r\n                    Statement\r\n                    <small class=\"page-info\">\r\n                        <i class=\"fa fa-angle-double-right text-80\"></i>\r\n                        ID: [StatementId]\r\n                    </small>\r\n                </h1>\r\n            </div>\r\n\r\n            <div class=\"container px-0\">\r\n                <div class=\"row mt-4\">\r\n                    <div class=\"col-12 col-lg-10 offset-lg-1\">\r\n                        <div class=\"row\">\r\n                            <div class=\"col-12\">\r\n                                <div class=\"text-center text-150\">\r\n                                    <i class=\"fas fa-book fa-2x text-success-m2 mr-1\"></i>\r\n                                    <span class=\"text-default-d3\">[EstateName]</span>\r\n                                </div>\r\n                            </div>\r\n                        </div>\r\n                        <!-- .row -->\r\n\r\n                        <hr class=\"row brc-default-l1 mx-n1 mb-4\" />\r\n\r\n                        <div class=\"row\">\r\n                            <div class=\"col-sm-6\">\r\n                                <div>\r\n                                    <span class=\"text-sm text-grey-m2 align-middle\">To:</span>\r\n                                    <span class=\"text-600 text-110 text-blue align-middle\">[MerchantName]</span>\r\n                                </div>\r\n                                <div class=\"text-grey-m2\">\r\n                                    <div class=\"my-1\">\r\n                                        [MerchantAddressLine1]\r\n                                    </div>\r\n                                    <div class=\"my-1\">\r\n                                        [MerchantTown], [MerchantRegion]\r\n                                    </div>\r\n                                    <div class=\"my-1\">\r\n                                        [MerchantCountry], [MerchantPostcode]\r\n                                    </div>\r\n                                    <div class=\"my-1\"><i class=\"fa fa-phone fa-flip-horizontal text-secondary\"></i> <b class=\"text-600\">[MerchantContactNumber]</b></div>\r\n                                </div>\r\n                            </div>\r\n                            <!-- /.col -->\r\n\r\n                            <div class=\"text-95 col-sm-6 align-self-start d-sm-flex justify-content-end\">\r\n                                <hr class=\"d-sm-none\" />\r\n                                <div class=\"text-grey-m2\">\r\n                                    <div class=\"mt-1 mb-2 text-secondary-m1 text-600 text-125\">\r\n                                        Statement\r\n                                    </div>\r\n\r\n                                    <div class=\"my-2\"><i class=\"fa fa-circle text-blue-m2 text-xs mr-1\"></i> <span class=\"text-600 text-90\">ID:</span> #[StatementId]</div>\r\n\r\n                                    <div class=\"my-2\"><i class=\"fa fa-circle text-blue-m2 text-xs mr-1\"></i> <span class=\"text-600 text-90\">Date:</span> [StatementDate]</div>\r\n\r\n                                </div>\r\n                            </div>\r\n                            <!-- /.col -->\r\n                        </div>\r\n\r\n                        <div class=\"mt-4\">\r\n                            <div class=\"row text-600 text-white bgc-default-tp1 py-25\">\r\n                                <div class=\"d-none d-sm-block col-1\">#</div>\r\n                                <div class=\"d-none d-sm-block col-4\">Date</div>\r\n                                <div class=\"col-9 col-sm-5\">Description</div>\r\n                                <div class=\"d-none d-sm-block col-4 col-sm-2\">Amount</div>\r\n                            </div>\r\n\r\n                            <div class=\"text-95 text-secondary-d3\">\r\n                                [StatementLinesData]\r\n                            </div>\r\n\r\n                            <hr>\r\n\r\n                            <div class=\"row border-b-2 brc-default-l2\"></div>\r\n\r\n                            <div class=\"row mt-3\">\r\n                                <div class=\"col-12 col-sm-7 text-grey-d2 text-95 mt-2 mt-lg-0\">\r\n\r\n                                </div>\r\n\r\n                                <div class=\"col-12 col-sm-5 text-grey text-90 order-first order-sm-last\">\r\n                                    <div class=\"row my-2\">\r\n                                        <div class=\"col-12 text-center\">\r\n                                            <b>Statement Summary</b>\r\n                                        </div>\r\n                                    </div>\r\n                                    <div class=\"row my-2\">\r\n                                        <div class=\"col-5 text-right\">\r\n                                            Total:\r\n                                        </div>\r\n                                        <div class=\"col-7\">\r\n                                            <span class=\"text-120 text-secondary-d1\">[StatementTotal]</span>\r\n                                        </div>\r\n                                    </div>\r\n\r\n                                    <div class=\"row my-2\">\r\n                                        <div class=\"col-5 text-right\">\r\n                                            Transactions:\r\n                                        </div>\r\n                                        <div class=\"col-7\">\r\n                                            <span class=\"text-120 text-secondary-d1\">[TransactionsValue]</span>\r\n                                        </div>\r\n                                    </div>\r\n\r\n                                    <div class=\"row my-2\">\r\n                                        <div class=\"col-5 text-right\">\r\n                                            Fees:\r\n                                        </div>\r\n                                        <div class=\"col-7\">\r\n                                            <span class=\"text-120 text-secondary-d1\">[TransactionFeesValue]</span>\r\n                                        </div>\r\n                                    </div>\r\n                                </div>\r\n                            </div>\r\n\r\n                            <hr />\r\n\r\n                            <div>\r\n                                <span class=\"text-secondary-d1 text-105\">Thank you for your business</span>\r\n                            </div>\r\n                        </div>\r\n                    </div>\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </body>").ReturnsAsync("<tr>[StatementLineNumber][StatementLineDate][StatementLineDescription][StatementLineAmount]</tr>").ReturnsAsync("body{color:red;}").ReturnsAsync(".fa{display:inline;}").ReturnsAsync(".fa-solid{font-weight:bold;}");
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
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 1), 100, 1000.00m, 100, 10.00m, 1, 1000, 1, 200);
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 2), 200, 2000.00m, 200, 20.00m, 2, 1000, 2, 200);
            
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
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 1), 100, 1000.00m, 100, 10.00m, 1, 1000, 1, 200);
            merchantStatementAggregate.AddDailySummaryRecord(new DateTime(2025, 5, 2), 200, 2000.00m, 200, 20.00m, 2, 1000, 2, 200);
            merchantStatementAggregate.GenerateStatement(TestData.GeneratedDateTime);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, "<html>statement</html>");
            // Act
            Result<String> htmlResult = await _builder.GetStatementHtml(merchantStatementAggregate, _merchant, _cancellationToken);

            htmlResult.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task GetStatementHtml_ThrowsIfTemplateMissing() {
            // Arrange
            _fileSystemMock.Setup(fs => fs.File.ReadAllTextAsync(It.Is<string>(s => s.Contains("statement.html")), It.IsAny<CancellationToken>())).ThrowsAsync(new FileNotFoundException());

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