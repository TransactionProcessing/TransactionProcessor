using Moq;
using Moq.Protected;
using Shared.Logger;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_DepositFailure_ReturnsInvalidResult()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("deposit rejected")
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "deposit" },
                { "AgencyBankingAccountNumber", "123456789" },
                { "Amount", "250.75" }
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
            result.Message.ShouldBe("Failed to process deposit deposit rejected");

            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/deposit"));

            String requestBody = await capturedRequest.Content.ReadAsStringAsync();
            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            requestJson.RootElement.GetProperty("agentId").GetString().ShouldBe(TestData.Merchant.MerchantId.ToString());
            requestJson.RootElement.GetProperty("accountNumber").GetString().ShouldBe("123456789");
            requestJson.RootElement.GetProperty("amount").GetDecimal().ShouldBe(250.75m);
            requestJson.RootElement.GetProperty("transactionId").GetString().ShouldBe(TestData.TransactionId.ToString());
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_DepositMissingAmount_ReturnsInvalidResult()
        {
            HttpClient httpClient = SetupMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "deposit" },
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
            result.Message.ShouldBe("Amount - Amount is a required field for this transaction type");
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_DepositInvalidAmount_ReturnsInvalidResult()
        {
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpClient httpClient = new HttpClient(handlerMock.Object);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "deposit" },
                { "AgencyBankingAccountNumber", "123456789" },
                { "Amount", "not-a-number" }
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
            result.Message.ShouldBe("Amount - Amount is not a valid decimal value");
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_DepositSuccess_ReturnsMappedOperatorResponse()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ResponseCode\":\"0000\",\"TransactionId\":\"abc123\"}")
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "deposit" },
                { "AgencyBankingAccountNumber", "123456789" },
                { "Amount", "250.75" }
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
            result.Data.ResponseMessage.ShouldBe("SUCCESS");
            result.Data.AdditionalTransactionResponseMetadata.ShouldBeEmpty();

            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/deposit"));

            String requestBody = await capturedRequest.Content.ReadAsStringAsync();
            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            requestJson.RootElement.GetProperty("agentId").GetString().ShouldBe(TestData.Merchant.MerchantId.ToString());
            requestJson.RootElement.GetProperty("accountNumber").GetString().ShouldBe("123456789");
            requestJson.RootElement.GetProperty("amount").GetDecimal().ShouldBe(250.75m);
            requestJson.RootElement.GetProperty("transactionId").GetString().ShouldBe(TestData.TransactionId.ToString());
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_WithdrawalFailure_ReturnsInvalidResult()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("withdrawal rejected")
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "withdrawal" },
                { "AgencyBankingAccountNumber", "123456789" },
                { "Amount", "250.75" }
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
            result.Message.ShouldBe("Failed to process withdrawal withdrawal rejected");

            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/withdrawal"));

            String requestBody = await capturedRequest.Content.ReadAsStringAsync();
            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            requestJson.RootElement.GetProperty("agentId").GetString().ShouldBe(TestData.Merchant.MerchantId.ToString());
            requestJson.RootElement.GetProperty("accountNumber").GetString().ShouldBe("123456789");
            requestJson.RootElement.GetProperty("amount").GetDecimal().ShouldBe(250.75m);
            requestJson.RootElement.GetProperty("transactionId").GetString().ShouldBe(TestData.TransactionId.ToString());
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_WithdrawalMissingAmount_ReturnsInvalidResult()
        {
            HttpClient httpClient = SetupMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "withdrawal" },
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
            result.Message.ShouldBe("Amount - Amount is a required field for this transaction type");
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_WithdrawalInvalidAmount_ReturnsInvalidResult()
        {
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            HttpClient httpClient = new HttpClient(handlerMock.Object);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "withdrawal" },
                { "AgencyBankingAccountNumber", "123456789" },
                { "Amount", "not-a-number" }
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
            result.Message.ShouldBe("Amount - Amount is not a valid decimal value");
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_WithdrawalSuccess_ReturnsMappedOperatorResponse()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ResponseCode\":\"0000\",\"TransactionId\":\"abc123\"}")
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "withdrawal" },
                { "AgencyBankingAccountNumber", "123456789" },
                { "Amount", "250.75" }
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
            result.Data.ResponseMessage.ShouldBe("SUCCESS");
            result.Data.AdditionalTransactionResponseMetadata.ShouldBeEmpty();

            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/withdrawal"));

            String requestBody = await capturedRequest.Content.ReadAsStringAsync();
            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            requestJson.RootElement.GetProperty("agentId").GetString().ShouldBe(TestData.Merchant.MerchantId.ToString());
            requestJson.RootElement.GetProperty("accountNumber").GetString().ShouldBe("123456789");
            requestJson.RootElement.GetProperty("amount").GetDecimal().ShouldBe(250.75m);
            requestJson.RootElement.GetProperty("transactionId").GetString().ShouldBe(TestData.TransactionId.ToString());
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_WithdrawalSuccess_WithMixedCaseMetadata_ReturnsMappedOperatorResponse()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ResponseCode\":\"0000\",\"TransactionId\":\"abc123\"}")
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "agencybankingmessagetype", "withdrawal" },
                { "agencybankingaccountnumber", "123456789" },
                { "aMoUnT", "250.75" }
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
            result.Data.ResponseMessage.ShouldBe("SUCCESS");
            result.Data.AdditionalTransactionResponseMetadata.ShouldBeEmpty();

            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/withdrawal"));

            String requestBody = await capturedRequest.Content.ReadAsStringAsync();
            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            requestJson.RootElement.GetProperty("agentId").GetString().ShouldBe(TestData.Merchant.MerchantId.ToString());
            requestJson.RootElement.GetProperty("accountNumber").GetString().ShouldBe("123456789");
            requestJson.RootElement.GetProperty("amount").GetDecimal().ShouldBe(250.75m);
            requestJson.RootElement.GetProperty("transactionId").GetString().ShouldBe(TestData.TransactionId.ToString());
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_MiniStatementFailure_ReturnsInvalidResult()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("mini statement rejected")
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "AgencyBankingMessageType", "ministatement" },
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
            result.Message.ShouldBe("Failed to process mini-statement mini statement rejected");

            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/mini-statement"));

            String requestBody = await capturedRequest.Content.ReadAsStringAsync();
            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            requestJson.RootElement.GetProperty("agentId").GetString().ShouldBe(TestData.Merchant.MerchantId.ToString());
            requestJson.RootElement.GetProperty("accountNumber").GetString().ShouldBe("123456789");
            requestJson.RootElement.GetProperty("count").GetInt32().ShouldBe(5);
        }

        [Fact]
        public async Task AgencyBankingProxy_ProcessSaleMessage_MiniStatementSuccess_ReturnsMappedOperatorResponse()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "responseCode": "0000",
                  "responseMessage": "Mini statement successful",
                  "transactions": [
                    {
                      "transactionDate": "2026-07-21T00:00:00",
                      "transactionType": "D",
                      "amount": 25.00
                    },
                    {
                      "transactionDate": "2026-07-21T00:01:00",
                      "transactionType": "C",
                      "amount": 10.50
                    }
                  ]
                }
                """)
            };

            HttpRequestMessage capturedRequest = null;
            HttpClient httpClient = SetupMockHttpClient(responseMessage, request => capturedRequest = request);
            AgencyBankingConfiguration configuration = CreateConfiguration();
            IOperatorProxy proxy = new AgencyBankingProxy(httpClient, configuration);

            Dictionary<String, String> metadata = new Dictionary<String, String>
            {
                { "agencybankingmessagetype", "ministatement" },
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
            result.Data.ResponseMessage.ShouldBe("SUCCESS");
            result.Data.AdditionalTransactionResponseMetadata.ShouldContainKey("StatementLines");

            String statementLinesJson = result.Data.AdditionalTransactionResponseMetadata["StatementLines"];
            using JsonDocument statementLinesDocument = JsonDocument.Parse(statementLinesJson);
            statementLinesDocument.RootElement.ValueKind.ShouldBe(JsonValueKind.Array);
            statementLinesDocument.RootElement.GetArrayLength().ShouldBe(2);

            JsonElement firstLine = statementLinesDocument.RootElement[0];
            firstLine.EnumerateObject().Any(property => property.Value.ValueKind == JsonValueKind.String && property.Value.GetString() == "D").ShouldBeTrue();
            firstLine.EnumerateObject().Any(property => property.Value.ValueKind == JsonValueKind.Number && property.Value.GetDecimal() == 25.00m).ShouldBeTrue();

            JsonElement secondLine = statementLinesDocument.RootElement[1];
            secondLine.EnumerateObject().Any(property => property.Value.ValueKind == JsonValueKind.String && property.Value.GetString() == "C").ShouldBeTrue();
            secondLine.EnumerateObject().Any(property => property.Value.ValueKind == JsonValueKind.Number && property.Value.GetDecimal() == 10.50m).ShouldBeTrue();

            capturedRequest.ShouldNotBeNull();
            capturedRequest.Method.ShouldBe(HttpMethod.Post);
            capturedRequest.RequestUri.ShouldBe(new Uri("http://localhost/transactions/mini-statement"));

            String requestBody = await capturedRequest.Content.ReadAsStringAsync();
            using JsonDocument requestJson = JsonDocument.Parse(requestBody);
            requestJson.RootElement.GetProperty("agentId").GetString().ShouldBe(TestData.Merchant.MerchantId.ToString());
            requestJson.RootElement.GetProperty("accountNumber").GetString().ShouldBe("123456789");
            requestJson.RootElement.GetProperty("count").GetInt32().ShouldBe(5);
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
