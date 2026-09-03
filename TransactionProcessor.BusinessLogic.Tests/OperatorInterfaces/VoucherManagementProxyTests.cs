using Shared.Logger;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Tests.OperatorInterfaces
{
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.VoucherManagement;
    using MediatR;
    using Imposter.Abstractions;
    using Requests;
    using Shared.Serialisation;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Testing;
    using Xunit;

    public class VoucherManagementProxyTests
    {
        public VoucherManagementProxyTests() {
            Logger.Initialise(new NullLogger());
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        }

        [Fact]
        public async Task VoucherManagementProxy_ProcessLogonMessage_NullReturned() {
            IMediatorImposter mediator = new();
            
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Instance());

            var processLogonMessageResult = await voucherManagementProxy.ProcessLogonMessage(TestContext.Current.CancellationToken);
            processLogonMessageResult.IsSuccess.ShouldBeTrue();
            OperatorResponse operatorResponse = processLogonMessageResult.Data;
            operatorResponse.ShouldBeNull();
        }
        
        [Fact]
        public async Task VoucherManagementProxy_ProcessSaleMessage_VoucherIssueSuccessful_SaleMessageIsProcessed() {
            IMediatorImposter mediator = new();
            mediator.Send<Result<TransactionProcessor.Models.IssueVoucherResponse>>(Arg<IRequest<Result<TransactionProcessor.Models.IssueVoucherResponse>>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.IssueVoucherResponse));
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Instance());

            var result = await voucherManagementProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForVoucher(),
                                                                                                TestContext.Current.CancellationToken);
            result.IsSuccess.ShouldBeTrue();
            var operatorResponse = result.Data;
            operatorResponse.ShouldNotBeNull();
            operatorResponse.IsSuccessful.ShouldBeTrue();
            operatorResponse.ResponseCode.ShouldBe("0000");
            operatorResponse.ResponseMessage.ShouldBe("SUCCESS");
            operatorResponse.AdditionalTransactionResponseMetadata.ShouldContainKey("VoucherCode");
            operatorResponse.AdditionalTransactionResponseMetadata.ShouldContainKey("VoucherExpiryDate");
        }

        [Fact]
        public async Task VoucherManagementProxy_ProcessSaleMessage_VoucherIssueFailed_FailedResultReturned()
        {
            IMediatorImposter mediator = new();
            mediator.Send<Result<TransactionProcessor.Models.IssueVoucherResponse>>(Arg<IRequest<Result<TransactionProcessor.Models.IssueVoucherResponse>>>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure("Some grim error"));
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Instance());

            var result = await voucherManagementProxy.ProcessSaleMessage(TestData.TransactionId,
                TestData.OperatorId,
                TestData.Merchant,
                TestData.TransactionDateTime,
                TestData.TransactionReference,
                TestData.AdditionalTransactionMetaDataForVoucher(),
                TestContext.Current.CancellationToken);
            
            result.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherManagementProxy_ProcessSaleMessage_VoucherIssueThrowsTimeout_FailedResultReturned()
        {
            IMediatorImposter mediator = new();
            mediator.Send<Result<TransactionProcessor.Models.IssueVoucherResponse>>(Arg<IRequest<Result<TransactionProcessor.Models.IssueVoucherResponse>>>.Any(), Arg<CancellationToken>.Any())
                    .Throws(new TimeoutException("Execution Timeout Expired"));
            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Instance());

            var result = await voucherManagementProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                         TestData.OperatorId,
                                                                                         TestData.Merchant,
                                                                                         TestData.TransactionDateTime,
                                                                                         TestData.TransactionReference,
                                                                                         TestData.AdditionalTransactionMetaDataForVoucher(),
                                                                                         TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Failure);
            result.Message.ShouldContain("Error issuing voucher");
            result.Message.ShouldContain("Execution Timeout Expired");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("A")]
        public async Task VoucherManagementProxy_ProcessSaleMessage_InvalidData_TransactionAmount_ErrorThrown(String transactionAmount)
        {
            IMediatorImposter mediator = new();

            Dictionary<String, String> additionalMetatdata = TestData.AdditionalTransactionMetaDataForVoucher();
            additionalMetatdata["Amount"] = transactionAmount;

            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Instance());

            var result = await voucherManagementProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                        TestData.OperatorId,
                                                                                        TestData.Merchant,
                                                                                        TestData.TransactionDateTime,
                                                                                        TestData.TransactionReference,
                                                                                        additionalMetatdata,
                                                                                        TestContext.Current.CancellationToken);


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
            IMediatorImposter mediator = new();

            Dictionary<String, String> additionalMetatdata = new Dictionary<String, String>
                                                             {
                                                                 {"Amount", "10.00"},
                                                                 {"RecipientEmail", recipientEmail },
                                                                 {"RecipientMobile", recipientMobile}

                                                             };

            IOperatorProxy voucherManagementProxy = new VoucherManagementProxy(mediator.Instance());

            var result = await voucherManagementProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                        TestData.OperatorId,
                                                                                        TestData.Merchant,
                                                                                        TestData.TransactionDateTime,
                                                                                        TestData.TransactionReference,
                                                                                        additionalMetatdata,
                                                                                        TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
    }
}

