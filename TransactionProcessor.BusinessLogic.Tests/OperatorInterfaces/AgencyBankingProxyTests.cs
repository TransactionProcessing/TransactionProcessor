using Moq;
using Moq.Protected;
using Shared.Logger;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.OperatorInterfaces;
using TransactionProcessor.BusinessLogic.OperatorInterfaces.AgencyBanking;
using TransactionProcessor.BusinessLogic.OperatorInterfaces.SafaricomPinless;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.OperatorInterfaces
{
    public class AgencyBankingProxyTests
    {
        public AgencyBankingProxyTests()
        {
            Logger.Initialise(NullLogger.Instance);
            StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessLogonMessage_ReturnsSuccessfulResult()
        {
            HttpClient httpClient = SetupMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            var result = await proxy.ProcessLogonMessage(CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldBeNull();
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_MissingMessageType_ReturnsInvalidResult()
        {
            HttpClient httpClient = SetupMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingAccountNumber", "123456789" }
            };

            var result = await proxy.ProcessSaleMessage(TestData.TransactionId,
                                                        TestData.OperatorId,
                                                        TestData.Merchant,
                                                        TestData.TransactionDateTime,
                                                        TestData.TransactionReference,
                                                        metadata,
                                                        CancellationToken.None);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("AgencyBankingMessageType - Message Type is a required field for this transaction type");
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_MissingAccountNumber_ReturnsInvalidResult()
        {
            HttpClient httpClient = SetupMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "balanceenquiry" }
            };

            var result = await proxy.ProcessSaleMessage(TestData.TransactionId,
                                                        TestData.OperatorId,
                                                        TestData.Merchant,
                                                        TestData.TransactionDateTime,
                                                        TestData.TransactionReference,
                                                        metadata,
                                                        CancellationToken.None);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("AgencyBankingAccountNumber - Account Number is a required field for this transaction type");
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_UnsupportedMessageType_ReturnsInvalidResult()
        {
            HttpClient httpClient = SetupMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "cashwithdrawal" },
                { "AgencyBankingAccountNumber", "123456789" }
            };

            var result = await proxy.ProcessSaleMessage(TestData.TransactionId,
                                                        TestData.OperatorId,
                                                        TestData.Merchant,
                                                        TestData.TransactionDateTime,
                                                        TestData.TransactionReference,
                                                        metadata,
                                                        CancellationToken.None);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("AgencyBankingMessageType - Message Type cashwithdrawal is not supported for this transaction type");
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_BalanceEnquiryFailure_ReturnsInvalidResult()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("operator unavailable")
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "balanceenquiry" },
                { "AgencyBankingAccountNumber", "123456789" }
            };

            var result = await proxy.ProcessSaleMessage(TestData.TransactionId,
                                                        TestData.OperatorId,
                                                        TestData.Merchant,
                                                        TestData.TransactionDateTime,
                                                        TestData.TransactionReference,
                                                        metadata,
                                                        CancellationToken.None);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("Failed to process balance enquiry operator unavailable");
            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/balance-enquiry"));
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_BalanceEnquirySuccess_ReturnsMappedOperatorResponse()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ResponseCode\":\"0000\",\"ResponseMessage\":\"Balance enquiry successful\",\"AvailableBalance\":2500}")
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "agencybankingmessagetype", "balanceenquiry" },
                { "agencybankingaccountnumber", "123456789" }
            };

            var result = await proxy.ProcessSaleMessage(TestData.TransactionId,
                                                        TestData.OperatorId,
                                                        TestData.Merchant,
                                                        TestData.TransactionDateTime,
                                                        TestData.TransactionReference,
                                                        metadata,
                                                        CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.IsSuccessful.ShouldBeTrue();
            result.Data.ResponseCode.ShouldBe("0000");
            result.Data.AdditionalTransactionResponseMetadata.ShouldContainKey("AgencyBankingBalance");

            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/balance-enquiry"));

            String requestBody = await capturedRequest.Content.ReadAsStringAsync();
            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            requestJson.RootElement.GetProperty("agentId").GetString().ShouldBe(TestData.Merchant.MerchantId.ToString());
            requestJson.RootElement.GetProperty("accountNumber").GetString().ShouldBe("123456789");
        }

        private static AgencyBankingConfiguration CreateConfiguration()
        {
            return new AgencyBankingConfiguration
            {
                Url = "http://localhost"
            };
        }

        private static HttpClient SetupMockHttpClient(HttpResponseMessage responseMessage, Action<HttpRequestMessage> onRequest = null)
        {
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .Callback<HttpRequestMessage, CancellationToken>((request, cancellationToken) => onRequest?.Invoke(request))
                       .ReturnsAsync(responseMessage);

            return new HttpClient(handlerMock.Object);
        }
    }
}
