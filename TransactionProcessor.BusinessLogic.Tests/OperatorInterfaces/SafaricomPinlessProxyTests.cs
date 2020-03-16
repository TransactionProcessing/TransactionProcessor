﻿using System;
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
    using Shouldly;
    using Testing;
    using Xunit;

    public class SafaricomPinlessProxyTests
    {
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

            OperatorResponse operatorResponse = await  safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.AdditionalTransactionMetaData,
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

            OperatorResponse operatorResponse = await safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                               TestData.Merchant,
                                                                                               TestData.TransactionDateTime,
                                                                                               TestData.AdditionalTransactionMetaData,
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
                                        await safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                       TestData.Merchant,
                                                                                       TestData.TransactionDateTime,
                                                                                       TestData.AdditionalTransactionMetaData,
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
