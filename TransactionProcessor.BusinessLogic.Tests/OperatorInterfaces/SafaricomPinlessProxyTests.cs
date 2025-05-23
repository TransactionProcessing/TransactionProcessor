﻿using System;
using System.Collections.Generic;
using SimpleResults;

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
            var processLogonMessageResult = await safaricomPinlessproxy.ProcessLogonMessage(CancellationToken.None);
            processLogonMessageResult.IsSuccess.ShouldBeTrue();
            OperatorResponse operatorResponse = processLogonMessageResult.Data;
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

            var result= await  safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                                CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.IsSuccessful.ShouldBeTrue();
            result.Data.ResponseCode.ShouldBe("200");
            result.Data.ResponseMessage.ShouldBe("Topup Successful");
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

            var result = await safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForMobileTopup(amountName: amountFieldName),
                                                                                                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.IsSuccessful.ShouldBeTrue();
            result.Data.ResponseCode.ShouldBe("200");
            result.Data.ResponseMessage.ShouldBe("Topup Successful");
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

            var result= await safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                                TestData.OperatorId,
                                                                                                TestData.Merchant,
                                                                                                TestData.TransactionDateTime,
                                                                                                TestData.TransactionReference,
                                                                                                TestData.AdditionalTransactionMetaDataForMobileTopup(customerAccountNumberName: customerAccountNumberFieldName),
                                                                                                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.IsSuccessful.ShouldBeTrue();
            result.Data.ResponseCode.ShouldBe("200");
            result.Data.ResponseMessage.ShouldBe("Topup Successful");
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

            var result =  await safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                               TestData.OperatorId,
                                                                                               TestData.Merchant,
                                                                                               TestData.TransactionDateTime,
                                                                                               TestData.TransactionReference,
                                                                                               TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                               CancellationToken.None);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Failure);
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

            var result = await safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                       TestData.OperatorId,
                                                                                       TestData.Merchant,
                                                                                       TestData.TransactionDateTime,
                                                                                       TestData.TransactionReference,
                                                                                       TestData.AdditionalTransactionMetaDataForMobileTopup(),
                                                                                       CancellationToken.None);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Failure);
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

            var result = await safaricomPinlessproxy.ProcessSaleMessage(TestData.TransactionId,
                                                                                       TestData.OperatorId,
                                                                                       TestData.Merchant,
                                                                                       TestData.TransactionDateTime,
                                                                                       TestData.TransactionReference,
                                                                                       additionalMetatdata,
                                                                                       CancellationToken.None);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
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
