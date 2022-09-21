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
    using Microsoft.Extensions.Caching.Memory;
    using Moq;
    using PataPawaPostPay;
    using Shouldly;
    using Testing;
    using TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPostPay;
    using Xunit;

    public class PataPawaPostPayProxyTests
    {
        [Fact]
        public async Task PataPawaPostPayProxy_ProcessLogonMessage_SuccessfulResponse_MessageIsProcessed() {

            Mock< IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            service.Setup(s => s.getLoginRequestAsync(It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(TestData.PataPawaPostPaidSuccessfulLoginResponse);
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();
            
            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            OperatorResponse logonResponse = await proxy.ProcessLogonMessage("", CancellationToken.None);

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

            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            service.Setup(s => s.getLoginRequestAsync(It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(TestData.PataPawaPostPaidFailedLoginResponse);
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            OperatorResponse logonResponse = await proxy.ProcessLogonMessage(TestData.TokenResponse().AccessToken, CancellationToken.None);

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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            service.Setup(s => s.getVerifyRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()))
                         .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse);
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);
            
            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                           TestData.TransactionId,
                                                                           TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            service.Setup(s => s.getVerifyRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()))
                         .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulVerifyAccountResponse);
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidFailedLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                   TestData.TransactionId,
                                                                                   TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                    TestData.TransactionId,
                    TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                   TestData.TransactionId,
                                                                                   TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(async () => {
                                                                                           OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                               TestData.TransactionId,
                                                                                               TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            service.Setup(s => s.getVerifyRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>()))
                         .ReturnsAsync(TestData.PataPawaPostPaidFailedVerifyAccountResponse);
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            Exception ex = Should.Throw<Exception>(async () => {
                                                       OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                           TestData.TransactionId,
                                                           TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            service.Setup(s => s.getPayBillRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(),
                                                        It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Decimal>()))
                   .ReturnsAsync(TestData.PataPawaPostPaidSuccessfulProcessBillResponse);
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);
            
            OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                           TestData.TransactionId,
                                                                           TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidFailedLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                   TestData.TransactionId,
                                                                                   TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                   TestData.TransactionId,
                                                                                   TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                    TestData.TransactionId,
                    TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                   TestData.TransactionId,
                                                                                   TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                    TestData.TransactionId,
                    TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentNullException ex = Should.Throw<ArgumentNullException>(async () => {
                                                                               OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                   TestData.TransactionId,
                                                                                   TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(async () => {
                                                                                           OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                               TestData.TransactionId,
                                                                                               TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);

            ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(async () => {
                                                                                           OperatorResponse saleResponse = await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                               TestData.TransactionId,
                                                                                               TestData.OperatorIdentifier1,
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
            Mock<IPataPawaPostPayService> service = new Mock<IPataPawaPostPayService>();
            service.Setup(s => s.getPayBillRequestAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(),
                                                        It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Decimal>()))
                   .ReturnsAsync(TestData.PataPawaPostPaidFailedProcessBillResponse);
            Mock<PataPawaPostPayServiceClient> serviceClient = new Mock<PataPawaPostPayServiceClient>();

            Func<PataPawaPostPayServiceClient, String, IPataPawaPostPayService> channelResolver = (client,
                                                                                                   s) => {
                                                                                                      return service.Object;
                                                                                                  };
            PataPawaPostPaidConfiguration configuration = TestData.PataPawaPostPaidConfiguration;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            memoryCache.Set("PataPawaPostPayLogon", TestData.PataPawaPostPaidSuccessfulLoginOperatorResponse);

            PataPawaPostPayProxy proxy = new PataPawaPostPayProxy(serviceClient.Object, channelResolver, configuration, memoryCache);
            

            Exception ex = Should.Throw<Exception>(async () => {
                                                       await proxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                      TestData.TransactionId,
                                                                                      TestData.OperatorIdentifier1,
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