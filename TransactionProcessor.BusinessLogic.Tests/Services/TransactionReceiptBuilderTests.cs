using System;
using System.Collections.Generic;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using BusinessLogic.Services;
    using Models;
    using Shared.Serialisation;
    using Shouldly;
    using System.IO;
    using System.IO.Abstractions.TestingHelpers;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Testing;
    using Xunit;

    public class TransactionReceiptBuilderTests
    {
        public TransactionReceiptBuilderTests() {
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        }

        [Fact]
        public async Task TransactionReceiptBuilder_GetEmailReceiptMessage_MessageBuilt()
        {
            Transaction transaction = new Transaction
                                      {
                                          OperatorId = TestData.OperatorId,
                                          TransactionNumber = "12345"
                                      };

            var path = Directory.GetParent(Assembly.GetExecutingAssembly().Location);

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                {
                                                    { $"{path}/Receipts/Email/OperatorName/TransactionAuthorised.html", new MockFileData("Transaction Number: [TransactionNumber]") }
                                                });

            TransactionReceiptBuilder receiptBuilder = new TransactionReceiptBuilder(fileSystem);

            String receiptMessage = await receiptBuilder.GetEmailReceiptMessage(transaction, new Models.Merchant.Merchant(),"OperatorName", CancellationToken.None);

            receiptMessage.ShouldBe("Transaction Number: 12345");


        }
    }
}
