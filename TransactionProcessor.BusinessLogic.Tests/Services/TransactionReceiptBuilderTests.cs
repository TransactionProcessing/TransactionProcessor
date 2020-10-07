using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.Services
{
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Services;
    using EstateManagement.DataTransferObjects.Responses;
    using Models;
    using Shouldly;
    using Xunit;

    public class TransactionReceiptBuilderTests
    {
        [Fact]
        public async Task TransactionReceiptBuilder_GetEmailReceiptMessage_MessageBuilt()
        {
            Transaction transaction = new Transaction
                                      {
                                          OperatorIdentifier = "Safaricom",
                                          TransactionNumber = "12345"
                                      };

            var path = Directory.GetParent(Assembly.GetExecutingAssembly().Location);

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                                                {
                                                    { $"{path}\\Receipts\\Email\\{transaction.OperatorIdentifier}\\TransactionAuthorised.html", new MockFileData("Transaction Number: [TransactionNumber]") }
                                                });

            TransactionReceiptBuilder receiptBuilder = new TransactionReceiptBuilder(fileSystem);

            String receiptMessage = await receiptBuilder.GetEmailReceiptMessage(transaction, new MerchantResponse(), CancellationToken.None);

            receiptMessage.ShouldBe("Transaction Number: 12345");


        }
    }
}
