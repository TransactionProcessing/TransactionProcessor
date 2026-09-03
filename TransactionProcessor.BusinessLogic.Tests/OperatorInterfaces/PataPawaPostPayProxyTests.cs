using SimpleResults;
using System;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Tests.OperatorInterfaces
{
    using Common;
    using Microsoft.Extensions.Caching.Memory;
    using Imposter.Abstractions;
    using PataPawaPostPay;
    using Shared.Serialisation;
    using Shouldly;
    using System.Text.Json;
    using System.Threading;
    using Testing;
    using TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPostPay;
    using Xunit;

    public class PataPawaPostPayProxyTests{
        private readonly IPataPawaPostPayServiceImposter PataPawaPostPayService;

        private readonly PataPawaPostPayServiceClientImposter PataPawaPostPayServiceClient;

        private readonly Func<PataPawaPostPayServiceClient, String, String, IPataPawaPostPayService> ChannelResolver;

        private readonly PataPawaPostPayProxy PataPawaPostPayProxy;

        private readonly IMemoryCache MemoryCache;

        public PataPawaPostPayProxyTests(){
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
            PataPawaPostPayService = new IPataPawaPostPayServiceImposter();
            PataPawaPostPayServiceClient = new PataPawaPostPayServiceClientImposter();

            ChannelResolver = (client,
                               clientName,
                               s) => {
                                  return PataPawaPostPayService.Instance();
                              };

            MemoryCache = new MemoryCache(new MemoryCacheOptions());
            PataPawaPostPayProxy = new PataPawaPostPayProxy(PataPawaPostPayServiceClient.Instance(), ChannelResolver, TestData.PataPawaPostPaidConfiguration, this.MemoryCache);
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessLogonMessage_SuccessfulResponse_MessageIsProcessed() {

            PataPawaPostPayService.getLoginRequestAsync(Arg<String>.Any(), Arg<String>.Any()).ReturnsAsync(TestData.PataPawaPostPaidSuccessfulLoginResponse);

            var logonResponseResult = await PataPawaPostPayProxy.ProcessLogonMessage(TestContext.Current.CancellationToken);
            logonResponseResult.IsSuccess.ShouldBeTrue();
            BusinessLogic.OperatorInterfaces.OperatorResponse logonResponse = logonResponseResult.Data;
            logonResponse.ShouldNotBeNull();
            logonResponse.IsSuccessful.ShouldBeTrue();
            logonResponse.ResponseMessage.ShouldBe(TestData.PataPawaPostPaidSuccessfulLoginResponse.message);
            logonResponse.ResponseCode.ShouldBe(TestData.PataPawaPostPaidSuccessfulLoginResponse.status.ToString().PadLeft(4, '0'));
            String apiKey = logonResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<String>("PataPawaPostPaidAPIKey");
            apiKey.ShouldNotBeNullOrEmpty();
            apiKey.ShouldBe(TestData.PataPawaPostPaidSuccessfulLoginResponse.api_key);
            Decimal balance = logonResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<Decimal>("PataPawaPostPaidBalance");
            balance.ShouldBe(TestData.PataPawaPostPaidSuccessfulLoginResponse.balance);
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessLogonMessage_LogonCached_SuccessfulResponse_MessageIsProcessed()
        {
            BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new() { TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString() };

            this.MemoryCache.Set("PataPawaPostPayLogon", operatorResponse, new MemoryCacheEntryOptions());

            var result = await PataPawaPostPayProxy.ProcessLogonMessage(TestContext.Current.CancellationToken);

            result.IsSuccess.ShouldBeTrue();
            result.Data.TransactionId.ShouldBe(operatorResponse.TransactionId);
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessLogonMessage_FailedResponse_MessageIsProcessed() {

            PataPawaPostPayService.getLoginRequestAsync(Arg<String>.Any(), Arg<String>.Any()).ReturnsAsync(TestData.PataPawaPostPaidFailedLoginResponse);
            
            var result = await this.PataPawaPostPayProxy.ProcessLogonMessage(TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Failure);
            result.Message.ShouldBe(TestData.PataPawaPostPaidFailedLoginResponse.message);
        }
        
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_SuccessfulResponse_MessageIsProcessed() {
            PataPawaPostPayService.getVerifyRequestAsync(Arg<String>.Any(), Arg<String>.Any(), Arg<String>.Any())
                                  .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse);
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            var processSaleMessageResult = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(),
                                                                                                TestContext.Current.CancellationToken);

            processSaleMessageResult.IsSuccess.ShouldBeTrue();
            BusinessLogic.OperatorInterfaces.OperatorResponse saleResponse = processSaleMessageResult.Data;
            saleResponse.ShouldNotBeNull();
            saleResponse.IsSuccessful.ShouldBeTrue();
            saleResponse.ResponseMessage.ShouldBe("SUCCESS");
            var billBalance = saleResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<Decimal>("CustomerBillBalance");
            billBalance.ShouldBe(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse.account_balance);
            String accountNumber = saleResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<String>("CustomerAccountNumber");
            accountNumber.ShouldBe(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse.account_no);
            String accountName = saleResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<String>("CustomerAccountName");
            accountName.ShouldBe(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse.account_name);
            DateTime billDueDate = saleResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<DateTime>("CustomerBillDueDate");
            billDueDate.ShouldBe(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse.due_date);
        }
        
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_FailedLogon_ErrorIsThrown() {
            this.PataPawaPostPayService.getVerifyRequestAsync(Arg<String>.Any(), Arg<String>.Any(), Arg<String>.Any())
                .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse);
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidFailedLoginOperatorResponse);
            
            var result = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                                                    TestData.OperatorId,
                                                                                                                                                                    TestData.Merchant,
                                                                                                                                                                    TestData.TransactionDateTime,
                                                                                                                                                                    TestData.TransactionReference,
                                                                                                                                                                    TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(),
                                                                                                                                                                    TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("PataPawaPostPaidAPIKey");
        }
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_MissingMessageTypeFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result= await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                    TestData.OperatorId,
                                                                                                    TestData.Merchant,
                                                                                                    TestData.TransactionDateTime,
                                                                                                    TestData.TransactionReference,
                                                                                                    TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount_NoMessageType(),
                                                                                                    TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("PataPawaPostPaidMessageType");
        }
        
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_MissingCustomerAccountNumberFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            var result= await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                                                    TestData.OperatorId,
                                                                                                                                                                    TestData.Merchant,
                                                                                                                                                                    TestData.TransactionDateTime,
                                                                                                                                                                    TestData.TransactionReference,
                                                                                                                                                                    TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount_NoCustomerAccountNumber(),
                                                                                                                                                                    TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("CustomerAccountNumber");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_InvalidMessageType_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result= await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                                                                TestData.OperatorId,
                                                                                                                                                                                TestData.Merchant,
                                                                                                                                                                                TestData.TransactionDateTime,
                                                                                                                                                                                TestData.TransactionReference,
                                                                                                                                                                                TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(pataPawaPostPaidMessageType:"Unknown"),
                                                                                                                                                                                TestContext.Current.CancellationToken);


            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("PataPawaPostPaidMessageType");
        }
      
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_RequestFailedAtHost_ErrorIsThrown()
        {
            this.PataPawaPostPayService.getVerifyRequestAsync(Arg<String>.Any(), Arg<String>.Any(), Arg<String>.Any())
                .ReturnsAsync(TestData.PataPawaPostPaidFailedVerifyAccountResponse);
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            var result= await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                            TestData.OperatorId,
                                                                                                                                            TestData.Merchant,
                                                                                                                                            TestData.TransactionDateTime,
                                                                                                                                            TestData.TransactionReference,
                                                                                                                                            TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(customerAccountNumber: TestData.PataPawaPostPaidAccountNumber),
                                                                                                                                            TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.NotFound);
            result.Message.Contains($"Error verifying account number {TestData.PataPawaPostPaidAccountNumber}");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_Timeout_ErrorIsThrown()
        {
            this.PataPawaPostPayService.getVerifyRequestAsync(Arg<String>.Any(), Arg<String>.Any(), Arg<String>.Any())
                .ThrowsAsync(new TimeoutException("Execution Timeout Expired"));
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                               TestData.OperatorId,
                                                                                               TestData.Merchant,
                                                                                               TestData.TransactionDateTime,
                                                                                               TestData.TransactionReference,
                                                                                               TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(customerAccountNumber: TestData.PataPawaPostPaidAccountNumber),
                                                                                               TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Failure);
            result.Message.ShouldContain($"Error verifying account number {TestData.PataPawaPostPaidAccountNumber}");
            result.Message.ShouldContain("Execution Timeout Expired");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_Timeout_ErrorIsThrown()
        {
            this.PataPawaPostPayService.getPayBillRequestAsync(Arg<String>.Any(), Arg<String>.Any(), Arg<String>.Any(),
                                                               Arg<String>.Any(), Arg<String>.Any(), Arg<Decimal>.Any())
                .ThrowsAsync(new TimeoutException("Execution Timeout Expired"));
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                               TestData.OperatorId,
                                                                                               TestData.Merchant,
                                                                                               TestData.TransactionDateTime,
                                                                                               TestData.TransactionReference,
                                                                                               TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(customerAccountNumber: TestData.PataPawaPostPaidAccountNumber),
                                                                                               TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Failure);
            result.Message.ShouldContain($"Error paying bill for account number {TestData.PataPawaPostPaidAccountNumber}");
            result.Message.ShouldContain("Execution Timeout Expired");
        }
        
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_SuccessfulResponse_MessageIsProcessed()
        {
            this.PataPawaPostPayService.getPayBillRequestAsync(Arg<String>.Any(), Arg<String>.Any(), Arg<String>.Any(),
                                                               Arg<String>.Any(), Arg<String>.Any(), Arg<Decimal>.Any())
                .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulProcessBillResponse);
            this.MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var processSaleMessageResult = await this.PataPawaPostPayProxy.ProcessSaleMessage(
                                                                                                TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(),
                                                                                                TestContext.Current.CancellationToken);
            processSaleMessageResult.IsSuccess.ShouldBeTrue();
            BusinessLogic.OperatorInterfaces.OperatorResponse saleResponse = processSaleMessageResult.Data;
            saleResponse.ShouldNotBeNull();
            saleResponse.IsSuccessful.ShouldBeTrue();
            saleResponse.ResponseMessage.ShouldBe(TestData.PataPawaPostPaidSuccessfulProcessBillResponse.msg);
            saleResponse.TransactionId.ShouldBe(TestData.PataPawaPostPaidSuccessfulProcessBillResponse.receipt_no);
            saleResponse.ResponseCode.ShouldBe(TestData.PataPawaPostPaidSuccessfulProcessBillResponse.rescode);
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_FailedLogon_ErrorIsThrown()
        {
            this.MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidFailedLoginOperatorResponse);

            var result = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId, TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime,
                TestData.TransactionReference, TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(),
                TestContext.Current.CancellationToken);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("PataPawaPostPaidAPIKey");
        }
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingMessageTypeFromMetadata_ErrorIsThrown()
        {
            this.MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                                             TestData.OperatorId,
                                                                                                                                                             TestData.Merchant,
                                                                                                                                                             TestData.TransactionDateTime,
                                                                                                                                                             TestData.TransactionReference,
                                                                                                                                                             TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoMessageType(),
                                                                                                                                                             TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("PataPawaPostPaidAPIKey");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingCustomerAccountNumberFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                              TestData.OperatorId,
                                                                                              TestData.Merchant,
                                                                                              TestData.TransactionDateTime,
                                                                                              TestData.TransactionReference,
                                                                                              TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoCustomerAccountNumber(),
                                                                                              TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("CustomerAccountNumber");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingMobileNumberFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                                             TestData.OperatorId,
                                                                                                                                                             TestData.Merchant,
                                                                                                                                                             TestData.TransactionDateTime,
                                                                                                                                                             TestData.TransactionReference,
                                                                                                                                                             TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoMobileNumber(),
                                                                                                                                                             TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("MobileNumber");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingCustomerNameFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                              TestData.OperatorId,
                                                                                              TestData.Merchant,
                                                                                              TestData.TransactionDateTime,
                                                                                              TestData.TransactionReference,
                                                                                              TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoCustomerName(),
                                                                                              TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("CustomerName");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingAmountFromMetadata_ErrorIsThrown()
        {
            this.MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            var result = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                                             TestData.OperatorId,
                                                                                                                                                             TestData.Merchant,
                                                                                                                                                             TestData.TransactionDateTime,
                                                                                                                                                             TestData.TransactionReference,
                                                                                                                                                             TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoAmount(),
                                                                                                                                                             TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("Amount");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_InvalidAmountFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result= await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                                                         TestData.OperatorId,
                                                                                                                                                                         TestData.Merchant,
                                                                                                                                                                         TestData.TransactionDateTime,
                                                                                                                                                                         TestData.TransactionReference,
                                                                                                                                                                         TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(pataPawaPostPaidAmount:"A1"),
                                                                                                                                                                         TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("Amount");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_InvalidMessageType_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            var result= await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                                                                                         TestData.OperatorId,
                                                                                                                                                                         TestData.Merchant,
                                                                                                                                                                         TestData.TransactionDateTime,
                                                                                                                                                                         TestData.TransactionReference,
                                                                                                                                                                         TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(pataPawaPostPaidMessageType: "Unknown"),
                                                                                                                                                                         TestContext.Current.CancellationToken);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.Contains("PataPawaPostPaidMessageType");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_RequestFailedAtHost_ErrorThrown()
        {
            this.PataPawaPostPayService.getPayBillRequestAsync(Arg<String>.Any(), Arg<String>.Any(), Arg<String>.Any(),
                                                               Arg<String>.Any(), Arg<String>.Any(), Arg<Decimal>.Any())
                .ReturnsAsync(TestData.PataPawaPostPaidFailedProcessBillResponse);
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            var result = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                     TestData.OperatorId,
                                                                                                     TestData.Merchant,
                                                                                                     TestData.TransactionDateTime,
                                                                                                     TestData.TransactionReference,
                                                                                                     TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(customerAccountNumber:TestData.PataPawaPostPaidAccountNumber),
                                                                                                     TestContext.Current.CancellationToken);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Failure);
            result.Message.Contains($"Error paying bill for account number {TestData.PataPawaPostPaidAccountNumber}");
        }
    }
}

