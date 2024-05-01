using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Tests.OperatorInterfaces
{
    using System.Threading;
    using BusinessLogic.OperatorInterfaces;
    using Common;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Caching.Memory;
    using Moq;
    using PataPawaPostPay;
    using Shouldly;
    using Testing;
    using TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPostPay;
    using Xunit;

    public class PataPawaPostPayProxyTests{
        private readonly Mock<IPataPawaPostPayService> PataPawaPostPayService;

        private readonly Mock<PataPawaPostPayServiceClient> PataPawaPostPayServiceClient;

        private readonly Func<PataPawaPostPayServiceClient, String, String, IPataPawaPostPayService> ChannelResolver;

        private readonly PataPawaPostPayProxy PataPawaPostPayProxy;

        private readonly IMemoryCache MemoryCache;

        public PataPawaPostPayProxyTests(){
            PataPawaPostPayService = new Mock<IPataPawaPostPayService>();
            PataPawaPostPayServiceClient = new Mock<PataPawaPostPayServiceClient>();

            ChannelResolver = (client,
                               clientName,
                               s) => {
                                  return PataPawaPostPayService.Object;
                              };

            MemoryCache = new MemoryCache(new MemoryCacheOptions());
            PataPawaPostPayProxy = new PataPawaPostPayProxy(PataPawaPostPayServiceClient.Object, ChannelResolver, TestData.PataPawaPostPaidConfiguration, this.MemoryCache);
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessLogonMessage_SuccessfulResponse_MessageIsProcessed() {

            PataPawaPostPayService.Setup(s => s.getLoginRequestAsync(It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(TestData.PataPawaPostPaidSuccessfulLoginResponse);

            OperatorResponse logonResponse = await PataPawaPostPayProxy.ProcessLogonMessage("", CancellationToken.None);

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
        public async Task PataPawaPostPayProxy_ProcessLogonMessage_FailedResponse_MessageIsProcessed() {

            PataPawaPostPayService.Setup(s => s.getLoginRequestAsync(It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(TestData.PataPawaPostPaidFailedLoginResponse);
            
            OperatorResponse logonResponse = await this.PataPawaPostPayProxy.ProcessLogonMessage(TestData.TokenResponse().AccessToken, CancellationToken.None);

            logonResponse.ShouldNotBeNull();
            logonResponse.IsSuccessful.ShouldBeFalse();
            logonResponse.ResponseMessage.ShouldBe(TestData.PataPawaPostPaidFailedLoginResponse.message);
            logonResponse.ResponseCode.ShouldBe(TestData.PataPawaPostPaidFailedLoginResponse.status.ToString());
            String apiKey = logonResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<String>("PataPawaPostPaidAPIKey");
            apiKey.ShouldBeNull();
            Decimal balance = logonResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<Decimal>("PataPawaPostPaidBalance");
            balance.ShouldBe(0);
        }
        
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_SuccessfulResponse_MessageIsProcessed() {
            PataPawaPostPayService.Setup(s => s.getVerifyRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()))
                                  .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse);
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            OperatorResponse saleResponse = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(),
                                                                                                CancellationToken.None);

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
            this.PataPawaPostPayService.Setup(s => s.getVerifyRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()))
                .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse);
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidFailedLoginOperatorResponse);
            
            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                                    TestData.TransactionId,
                                                                                                                                                                    TestData.OperatorId,
                                                                                                                                                                    TestData.Merchant,
                                                                                                                                                                    TestData.TransactionDateTime,
                                                                                                                                                                    TestData.TransactionReference,
                                                                                                                                                                    TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(),
                                                                                                                                                                    CancellationToken.None);
                                                                           });
            ex.ParamName.ShouldBe("PataPawaPostPaidAPIKey");
        }
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_MissingMessageTypeFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                OperatorResponse saleResponse = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                    TestData.TransactionId,
                                                                                                    TestData.OperatorId,
                                                                                                    TestData.Merchant,
                                                                                                    TestData.TransactionDateTime,
                                                                                                    TestData.TransactionReference,
                                                                                                    TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount_NoMessageType(),
                                                                                                    CancellationToken.None);
            });
            ex.ParamName.ShouldBe("PataPawaPostPaidMessageType");
        }
        
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_MissingCustomerAccountNumberFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                                    TestData.TransactionId,
                                                                                                                                                                    TestData.OperatorId,
                                                                                                                                                                    TestData.Merchant,
                                                                                                                                                                    TestData.TransactionDateTime,
                                                                                                                                                                    TestData.TransactionReference,
                                                                                                                                                                    TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount_NoCustomerAccountNumber(),
                                                                                                                                                                    CancellationToken.None);
                                                                           });
            ex.ParamName.ShouldBe("CustomerAccountNumber");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_InvalidMessageType_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(async () => {
                                                                                           OperatorResponse saleResponse = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                                                TestData.TransactionId,
                                                                                                                                                                                TestData.OperatorId,
                                                                                                                                                                                TestData.Merchant,
                                                                                                                                                                                TestData.TransactionDateTime,
                                                                                                                                                                                TestData.TransactionReference,
                                                                                                                                                                                TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(pataPawaPostPaidMessageType:"Unknown"),
                                                                                                                                                                                CancellationToken.None);
                                                                                       });

            ex.ParamName.ShouldBe("PataPawaPostPaidMessageType");
        }
      
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_VerifyAccount_RequestFailedAtHost_ErrorIsThrown()
        {
            this.PataPawaPostPayService.Setup(s => s.getVerifyRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()))
                .ReturnsAsync(TestData.PataPawaPostPaidFailedVerifyAccountResponse);
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            Exception ex = Should.Throw<Exception>(async () => {
                                                       OperatorResponse saleResponse = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                            TestData.TransactionId,
                                                                                                                                            TestData.OperatorId,
                                                                                                                                            TestData.Merchant,
                                                                                                                                            TestData.TransactionDateTime,
                                                                                                                                            TestData.TransactionReference,
                                                                                                                                            TestData.AdditionalTransactionMetaDataForPataPawaVerifyAccount(customerAccountNumber: TestData.PataPawaPostPaidAccountNumber),
                                                                                                                                            CancellationToken.None);
                                                   });
            ex.Message.ShouldBe($"Error verifying account number {TestData.PataPawaPostPaidAccountNumber}");
        }
        
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_SuccessfulResponse_MessageIsProcessed()
        {
            this.PataPawaPostPayService.Setup(s => s.getPayBillRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(),
                                                                             It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Decimal>()))
                .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulProcessBillResponse);
            this.MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            OperatorResponse saleResponse = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(),
                                                                                                CancellationToken.None);

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

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await this.PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                                    TestData.TransactionId,
                                                                                                                                                                    TestData.OperatorId,
                                                                                                                                                                    TestData.Merchant,
                                                                                                                                                                    TestData.TransactionDateTime,
                                                                                                                                                                    TestData.TransactionReference,
                                                                                                                                                                    TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(),
                                                                                                                                                                    CancellationToken.None);
                                                                           });
            ex.ParamName.ShouldBe("PataPawaPostPaidAPIKey");
        }
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingMessageTypeFromMetadata_ErrorIsThrown()
        {
            this.MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                             TestData.TransactionId,
                                                                                                                                                             TestData.OperatorId,
                                                                                                                                                             TestData.Merchant,
                                                                                                                                                             TestData.TransactionDateTime,
                                                                                                                                                             TestData.TransactionReference,
                                                                                                                                                             TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoMessageType(),
                                                                                                                                                             CancellationToken.None);
                                                                           });
            ex.ParamName.ShouldBe("PataPawaPostPaidMessageType");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingCustomerAccountNumberFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                OperatorResponse saleResponse = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                              TestData.TransactionId,
                                                                                              TestData.OperatorId,
                                                                                              TestData.Merchant,
                                                                                              TestData.TransactionDateTime,
                                                                                              TestData.TransactionReference,
                                                                                              TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoCustomerAccountNumber(),
                                                                                              CancellationToken.None);
            });
            ex.ParamName.ShouldBe("CustomerAccountNumber");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingMobileNumberFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                             TestData.TransactionId,
                                                                                                                                                             TestData.OperatorId,
                                                                                                                                                             TestData.Merchant,
                                                                                                                                                             TestData.TransactionDateTime,
                                                                                                                                                             TestData.TransactionReference,
                                                                                                                                                             TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoMobileNumber(),
                                                                                                                                                             CancellationToken.None);
                                                                           });
            ex.ParamName.ShouldBe("MobileNumber");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingCustomerNameFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                OperatorResponse saleResponse = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                              TestData.TransactionId,
                                                                                              TestData.OperatorId,
                                                                                              TestData.Merchant,
                                                                                              TestData.TransactionDateTime,
                                                                                              TestData.TransactionReference,
                                                                                              TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoCustomerName(),
                                                                                              CancellationToken.None);
            });
            ex.ParamName.ShouldBe("CustomerName");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_MissingAmountFromMetadata_ErrorIsThrown()
        {
            this.MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                             TestData.TransactionId,
                                                                                                                                                             TestData.OperatorId,
                                                                                                                                                             TestData.Merchant,
                                                                                                                                                             TestData.TransactionDateTime,
                                                                                                                                                             TestData.TransactionReference,
                                                                                                                                                             TestData.AdditionalTransactionMetaDataForPataPawaProcessBill_NoAmount(),
                                                                                                                                                             CancellationToken.None);
                                                                           });
            ex.ParamName.ShouldBe("Amount");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_InvalidAmountFromMetadata_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(async () => {
                                                                                           OperatorResponse saleResponse = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                                         TestData.TransactionId,
                                                                                                                                                                         TestData.OperatorId,
                                                                                                                                                                         TestData.Merchant,
                                                                                                                                                                         TestData.TransactionDateTime,
                                                                                                                                                                         TestData.TransactionReference,
                                                                                                                                                                         TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(pataPawaPostPaidAmount:"A1"),
                                                                                                                                                                         CancellationToken.None);
                                                                                       });
            ex.ParamName.ShouldBe("Amount");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_InvalidMessageType_ErrorIsThrown()
        {
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(async () => {
                                                                                           OperatorResponse saleResponse = await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                                                                                         TestData.TransactionId,
                                                                                                                                                                         TestData.OperatorId,
                                                                                                                                                                         TestData.Merchant,
                                                                                                                                                                         TestData.TransactionDateTime,
                                                                                                                                                                         TestData.TransactionReference,
                                                                                                                                                                         TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(pataPawaPostPaidMessageType: "Unknown"),
                                                                                                                                                                         CancellationToken.None);
                                                                                       });
            ex.ParamName.ShouldBe("PataPawaPostPaidMessageType");
        }

        [Fact]
        public async Task PataPawaPostPayProxy_ProcessSaleMessage_ProcessBill_RequestFailedAtHost_ErrorThrown()
        {
            this.PataPawaPostPayService.Setup(s => s.getPayBillRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(),
                                                                             It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Decimal>()))
                .ReturnsAsync(TestData.PataPawaPostPaidFailedProcessBillResponse);
            MemoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            Exception ex = Should.Throw<Exception>(async () => {
                                                       await PataPawaPostPayProxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                     TestData.TransactionId,
                                                                                                     TestData.OperatorId,
                                                                                                     TestData.Merchant,
                                                                                                     TestData.TransactionDateTime,
                                                                                                     TestData.TransactionReference,
                                                                                                     TestData.AdditionalTransactionMetaDataForPataPawaProcessBill(customerAccountNumber:TestData.PataPawaPostPaidAccountNumber),
                                                                                                     CancellationToken.None);

                                                   });
            ex.Message.ShouldBe($"Error paying bill for account number {TestData.PataPawaPostPaidAccountNumber}");
        }
    }
}