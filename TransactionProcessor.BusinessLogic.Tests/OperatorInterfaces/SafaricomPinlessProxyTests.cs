using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.OperatorInterfaces
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.OperatorInterfaces;
    using BusinessLogic.OperatorInterfaces.SafaricomPinless;
    using Moq;
    using Moq.Protected;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using Xunit;

    public class SafaricomPinlessProxyTests
    {
        public SafaricomPinlessProxyTests(){
            Logger.Initialise(NullLogger.Instance);
        }

        [Fact]
        public async Task SafaricomPinlessProxy_ProcessLogonMessage_NullIsReturned() {
            HttpResponseMessage responseMessage = new HttpResponseMessage
                                                  {
                                                      StatusCode = HttpStatusCode.OK,
                                                      Content = new StringContent(TestData.SuccessfulSafaricomTopup)
                                                  };
            SafaricomConfiguration safaricomConfiguration = TestData.SafaricomConfiguration;
            HttpClient httpClient = SetupMockHttpClient(responseMessage);

            IOperatorProxy safaricomPinlessproxy = new SafaricomPinlessProxy(safaricomConfiguration, httpClient);
            OperatorResponse operatorResponse = await safaricomPinlessproxy.ProcessLogonMessage(TestData.TokenResponse().AccessToken, CancellationToken.None);
            operatorResponse.ShouldBeNull();

        }

        [Fact]
        public async Task SafaricomPinlessProxy_ProcessSaleMessage_TopupSuccessful_SaleMessageIsProcessed()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage
                                                  {
                                                    StatusCode = HttpStatusCode.OK,
                                                    Content = new StringContent(TestData.SuccessfulSafaricomTopup)
                                                  };

            SafaricomConfiguration safaricomConfiguration = TestData.SafaricomConfiguration;
            HttpClient httpClient = SetupMockHttpClient(responseMessage);

            IOperatorProxy safaricomPinlessproxy = new SafaricomPinlessProxy(safaricomConfiguration, httpClient);

            OperatorResponse operatorResponse = await  safaricomPinlessproxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                TestData.TransactionId,
                                                                                                TestData.OperatorIdentifier1,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                CancellationToken.None);

            operatorResponse.ShouldNotBeNull();
            operatorResponse.IsSuccessful.ShouldBeTrue();
            operatorResponse.ResponseCode.ShouldBe("200");
            operatorResponse.ResponseMessage.ShouldBe("Topup Successful");
        }

        [Theory]
        [InlineData("amount")]
        [InlineData("Amount")]
        [InlineData("AMOUNT")]
        public async Task SafaricomPinlessProxy_ProcessSaleMessage_MetadataCasingTests_Amount_TopupSuccessful_SaleMessageIsProcessed(String amountFieldName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestData.SuccessfulSafaricomTopup)
            };

            SafaricomConfiguration safaricomConfiguration = TestData.SafaricomConfiguration;
            HttpClient httpClient = SetupMockHttpClient(responseMessage);

            IOperatorProxy safaricomPinlessproxy = new SafaricomPinlessProxy(safaricomConfiguration, httpClient);

            OperatorResponse operatorResponse = await safaricomPinlessproxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                TestData.TransactionId,
                                                                                                TestData.OperatorIdentifier1,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForMobileTopup(amountName: amountFieldName),
                                                                                                CancellationToken.None);

            operatorResponse.ShouldNotBeNull();
            operatorResponse.IsSuccessful.ShouldBeTrue();
            operatorResponse.ResponseCode.ShouldBe("200");
            operatorResponse.ResponseMessage.ShouldBe("Topup Successful");
        }

        [Theory]
        [InlineData("customeraccountnumber")]
        [InlineData("customerAccountNumber")]
        [InlineData("CustomerAccountNumber")]
        [InlineData("CUSTOMERACCOUNTNUMBER")]
        public async Task SafaricomPinlessProxy_ProcessSaleMessage_MetadataCasingTests_CustomerAccountNumber_TopupSuccessful_SaleMessageIsProcessed(String customerAccountNumberFieldName)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(TestData.SuccessfulSafaricomTopup)
            };

            SafaricomConfiguration safaricomConfiguration = TestData.SafaricomConfiguration;
            HttpClient httpClient = SetupMockHttpClient(responseMessage);

            IOperatorProxy safaricomPinlessproxy = new SafaricomPinlessProxy(safaricomConfiguration, httpClient);

            OperatorResponse operatorResponse = await safaricomPinlessproxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken,
                                                                                                TestData.TransactionId,
                                                                                                TestData.OperatorIdentifier1,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForMobileTopup(customerAccountNumberName: customerAccountNumberFieldName),
                                                                                                CancellationToken.None);

            operatorResponse.ShouldNotBeNull();
            operatorResponse.IsSuccessful.ShouldBeTrue();
            operatorResponse.ResponseCode.ShouldBe("200");
            operatorResponse.ResponseMessage.ShouldBe("Topup Successful");
        }

        [Fact]
        public async Task SafaricomPinlessProxy_ProcessSaleMessage_TopupFailed_SaleMessageIsProcessed()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage
                                                  {
                                                      StatusCode = HttpStatusCode.OK,
                                                      Content = new StringContent(TestData.FailedSafaricomTopup)
                                                  };

            SafaricomConfiguration safaricomConfiguration = TestData.SafaricomConfiguration;
            HttpClient httpClient = SetupMockHttpClient(responseMessage);

            IOperatorProxy safaricomPinlessproxy = new SafaricomPinlessProxy(safaricomConfiguration, httpClient);

            OperatorResponse operatorResponse = await safaricomPinlessproxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken, 
                                                                                               TestData.TransactionId,
                                                                                               TestData.OperatorIdentifier1,
                                                                                               TestData.Merchant,
                                                                                               TestData.TransactionDateTime,
                                                                                               TestData.TransactionReference,
                                                                                               TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                               CancellationToken.None);

            operatorResponse.ShouldNotBeNull();
            operatorResponse.IsSuccessful.ShouldBeFalse();
            operatorResponse.ResponseCode.ShouldBe("500");
            operatorResponse.ResponseMessage.ShouldBe("Topup failed");
        }

        [Fact]
        public async Task SafaricomPinlessProxy_ProcessSaleMessage_FailedToSend_ErrorThrown()
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage
                                                  {
                                                      StatusCode = HttpStatusCode.InternalServerError
                                                  };

            SafaricomConfiguration safaricomConfiguration = TestData.SafaricomConfiguration;
            HttpClient httpClient = SetupMockHttpClient(responseMessage);

            IOperatorProxy safaricomPinlessproxy = new SafaricomPinlessProxy(safaricomConfiguration, httpClient);

            Should.Throw<Exception>(async () =>
                                    {
                                        await safaricomPinlessproxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken, 
                                                                                       TestData.TransactionId,
                                                                                       TestData.OperatorIdentifier1,
                                                                                       TestData.Merchant,
                                                                                       TestData.TransactionDateTime,
                                                                                       TestData.TransactionReference,
                                                                                       TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                       CancellationToken.None);
                                    });
        }

        [Theory]
        [InlineData("", "123456789")]
        [InlineData(null, "123456789")]
        [InlineData("A", "123456789")]
        [InlineData("1000.00", "")]
        [InlineData("1000.00", null)]
        public async Task SafaricomPinlessProxy_ProcessSaleMessage_InvalidData_ErrorThrown(String transactionAmount, String customerAccountNumber)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage
                                                  {
                                                      StatusCode = HttpStatusCode.OK,
                                                      Content = new StringContent(TestData.SuccessfulSafaricomTopup)
                                                  };

            SafaricomConfiguration safaricomConfiguration = TestData.SafaricomConfiguration;
            HttpClient httpClient = SetupMockHttpClient(responseMessage);

            IOperatorProxy safaricomPinlessproxy = new SafaricomPinlessProxy(safaricomConfiguration, httpClient);

            Dictionary<String,String> additionalMetatdata = new Dictionary<String, String>
                                                            {
                                                                {"Amount", transactionAmount},
                                                                {"CustomerAccountNumber",customerAccountNumber }
                                                            };

            Should.Throw<Exception>(async () =>
                                    {
                                        await safaricomPinlessproxy.ProcessSaleMessage(TestData.TokenResponse().AccessToken, 
                                                                                       TestData.TransactionId,
                                                                                       TestData.OperatorIdentifier1,
                                                                                       TestData.Merchant,
                                                                                       TestData.TransactionDateTime,
                                                                                       TestData.TransactionReference,
                                                                                       additionalMetatdata,
                                                                                       CancellationToken.None);
                                    });
        }

        private HttpClient SetupMockHttpClient(HttpResponseMessage responseMessage)
        {
            Mock<HttpMessageHandler> handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                       .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handlerMock.Object)
                             {
                                 BaseAddress = new Uri("http://test.com")
                             };

            return httpClient;
        }
    }
}
