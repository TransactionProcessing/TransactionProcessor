namespace TransactionProcessor.BusinessLogic.Tests.OperatorInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.VoucherManagement;
    using Moq;
    using Shouldly;
    using Testing;
    using VoucherManagement.Client;
    using VoucherManagement.DataTransferObjects;
    using Xunit;

    public class VoucherManagementProxyTests
    {
        [Fact]
        public async Task VoucherManagementProxy_ProcessLogonMessage_NullReturned()
        {
            Mock<IVoucherManagementClient> voucherManagementClient = new Mock<IVoucherManagementClient>();
            voucherManagementClient.Setup(v => v.IssueVoucher(It.IsAny<String>(), It.IsAny<IssueVoucherRequest>(), It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(TestData.IssueVoucherResponse);
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(voucherManagementClient.Object);

            OperatorResponse operatorResponse = await voucherManagementProxy.ProcessLogonMessage(TestData.TokenResponse().AccessToken,
                                                                                                CancellationToken.None);

            operatorResponse.ShouldBeNull();
        }

        [Fact]
        public async Task VoucherManagementProxy_ProcessSaleMessage_VoucherIssueSuccessful_SaleMessageIsProcessed()
        {
            Mock<IVoucherManagementClient> voucherManagementClient = new Mock<IVoucherManagementClient>();
            voucherManagementClient.Setup(v => v.IssueVoucher(It.IsAny<String>(), It.IsAny<IssueVoucherRequest>(), It.IsAny<CancellationToken>()))
                                   .ReturnsAsync(TestData.IssueVoucherResponse);
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(voucherManagementClient.Object);

            OperatorResponse operatorResponse = await voucherManagementProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                TestData.TransactionId,
                                                                                                TestData.OperatorIdentifier1,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForVoucher(),
                                                                                                CancellationToken.None);

            operatorResponse.ShouldNotBeNull();
            operatorResponse.IsSuccessful.ShouldBeTrue();
            operatorResponse.ResponseCode.ShouldBe("0000");
            operatorResponse.ResponseMessage.ShouldBe("SUCCESS");
            operatorResponse.AdditionalTransactionResponseMetadata.ShouldContainKey("VoucherCode");
            operatorResponse.AdditionalTransactionResponseMetadata.ShouldContainKey("VoucherExpiryDate");
        }

        [Fact]
        public async Task VoucherManagementProxy_ProcessSaleMessage_FailedToSend_ErrorThrown()
        {
            Mock<IVoucherManagementClient> voucherManagementClient = new Mock<IVoucherManagementClient>();
            voucherManagementClient.Setup(v => v.IssueVoucher(It.IsAny<String>(), It.IsAny<IssueVoucherRequest>(), It.IsAny<CancellationToken>()))
                                   .ThrowsAsync(new Exception());
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(voucherManagementClient.Object);

            Should.Throw<Exception>(async () =>
                                    {
                                        await voucherManagementProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                        TestData.TransactionId,
                                                                                        TestData.OperatorIdentifier1,
                                                                                        TestData.Merchant,
                                                                                        TestData.TransactionDateTime,
                                                                                        TestData.TransactionReference,
                                                                                        TestData.AdditionalTransactionMetaDataForVoucher(),
                                                                                        CancellationToken.None);


                                    });
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("A")]
        public async Task VoucherManagementProxy_ProcessSaleMessage_InvalidData_TransactionAmount_ErrorThrown(String transactionAmount)
        {
            Mock<IVoucherManagementClient> voucherManagementClient = new Mock<IVoucherManagementClient>();

            Dictionary<String, String> additionalMetatdata = TestData.AdditionalTransactionMetaDataForVoucher();
            additionalMetatdata["Amount"] = transactionAmount;

            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(voucherManagementClient.Object);

            Should.Throw<Exception>(async () =>
                                    {
                                        await voucherManagementProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                        TestData.TransactionId,
                                                                                        TestData.OperatorIdentifier1,
                                                                                        TestData.Merchant,
                                                                                        TestData.TransactionDateTime,
                                                                                        TestData.TransactionReference,
                                                                                        additionalMetatdata,
                                                                                        CancellationToken.None);


                                    });
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData(null, null)]
        public async Task VoucherManagementProxy_ProcessSaleMessage_InvalidData_RecipientDetails_ErrorThrown(String recipientEmail, String recipientMobile)
        {
            Mock<IVoucherManagementClient> voucherManagementClient = new Mock<IVoucherManagementClient>();

            Dictionary<String, String> additionalMetatdata = new Dictionary<String, String>
                                                             {
                                                                 {"Amount", "10.00"},
                                                                 {"RecipientEmail", recipientEmail },
                                                                 {"RecipientMobile", recipientMobile}

                                                             };

            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(voucherManagementClient.Object);

            Should.Throw<Exception>(async () =>
                                    {
                                        await voucherManagementProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                        TestData.TransactionId,
                                                                                        TestData.OperatorIdentifier1,
                                                                                        TestData.Merchant,
                                                                                        TestData.TransactionDateTime,
                                                                                        TestData.TransactionReference,
                                                                                        additionalMetatdata,
                                                                                        CancellationToken.None);


                                    });
        }
    }
}