using SimpleResults;
using TransactionProcessor.DataTransferObjects;

namespace TransactionProcessor.BusinessLogic.Tests.OperatorInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.VoucherManagement;
    using MediatR;
    using Models;
    using Moq;
    using Requests;
    using Shouldly;
    using Testing;
    using Xunit;

    public class VoucherManagementProxyTests
    {
        [Fact]
        public async Task VoucherManagementProxy_ProcessLogonMessage_NullReturned() {
            Mock<IMediator> mediator = new Mock<IMediator>();
            
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Object);

            OperatorResponse operatorResponse = await voucherManagementProxy.ProcessLogonMessage(TestData.TokenResponse().AccessToken,
                                                                                                CancellationToken.None);

            operatorResponse.ShouldBeNull();
        }
        
        [Fact]
        public async Task VoucherManagementProxy_ProcessSaleMessage_VoucherIssueSuccessful_SaleMessageIsProcessed() {
            Mock<IMediator> mediator = new Mock<IMediator>();
            mediator.Setup(m => m.Send(It.IsAny<VoucherCommands.IssueVoucherCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.IssueVoucherResponse));
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Object);

            var result = await voucherManagementProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForVoucher(),
                                                                                                CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            var operatorResponse = result.Data;
            operatorResponse.ShouldNotBeNull();
            operatorResponse.IsSuccessful.ShouldBeTrue();
            operatorResponse.ResponseCode.ShouldBe("0000");
            operatorResponse.ResponseMessage.ShouldBe("SUCCESS");
            operatorResponse.AdditionalTransactionResponseMetadata.ShouldContainKey("VoucherCode");
            operatorResponse.AdditionalTransactionResponseMetadata.ShouldContainKey("VoucherExpiryDate");
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("A")]
        public async Task VoucherManagementProxy_ProcessSaleMessage_InvalidData_TransactionAmount_ErrorThrown(String transactionAmount)
        {
            Mock<IMediator> mediator = new Mock<IMediator>();

            Dictionary<String, String> additionalMetatdata = TestData.AdditionalTransactionMetaDataForVoucher();
            additionalMetatdata["Amount"] = transactionAmount;

            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Object);

            var result = await voucherManagementProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                        TestData.TransactionId,
                                                                                        TestData.OperatorId,
                                                                                        TestData.Merchant,
                                                                                        TestData.TransactionDateTime,
                                                                                        TestData.TransactionReference,
                                                                                        additionalMetatdata,
                                                                                        CancellationToken.None);


                                        result.IsFailed.ShouldBeTrue();
                                        result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", null)]
        [InlineData(null, "")]
        [InlineData(null, null)]
        public async Task VoucherManagementProxy_ProcessSaleMessage_InvalidData_RecipientDetails_ErrorThrown(String recipientEmail, String recipientMobile)
        {
            Mock<IMediator> mediator = new Mock<IMediator>();

            Dictionary<String, String> additionalMetatdata = new Dictionary<String, String>
                                                             {
                                                                 {"Amount", "10.00"},
                                                                 {"RecipientEmail", recipientEmail },
                                                                 {"RecipientMobile", recipientMobile}

                                                             };

            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Object);

            var result = await voucherManagementProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                        TestData.TransactionId,
                                                                                        TestData.OperatorId,
                                                                                        TestData.Merchant,
                                                                                        TestData.TransactionDateTime,
                                                                                        TestData.TransactionReference,
                                                                                        additionalMetatdata,
                                                                                        CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
    }
}