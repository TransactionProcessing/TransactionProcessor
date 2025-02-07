using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.OperatorInterfaces;
using TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.OperatorInterfaces;

public class PataPawaPrePayProxyTests {
    private readonly IOperatorProxy PataPawaPrePayProxy;
    private readonly MockHttpMessageHandler MockHttpMessageHandler;
    private readonly IMemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions());

    public PataPawaPrePayProxyTests() {
        PataPawaPrePaidConfiguration configuration = new PataPawaPrePaidConfiguration();
        configuration.Url = "http://localhost";
        configuration.Password = "password";
        configuration.Username = "username";

        this.MockHttpMessageHandler = new MockHttpMessageHandler();
        HttpClient httpClient = new HttpClient(this.MockHttpMessageHandler);

        this.PataPawaPrePayProxy = new PataPawaPrePayProxy(configuration, httpClient, this.MemoryCache);

        Logger.Initialise(NullLogger.Instance);
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessLogonMessage_MessageProcessed() {

        LogonResponse logonResponse = new LogonResponse { Balance = "0", Key = "Key", Msg = "Success", Status = 0 };

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(logonResponse));
            
        var result = await this.PataPawaPrePayProxy.ProcessLogonMessage(CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe("0000");
        result.Data.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessLogonMessage_CachedResponse_MessageProcessed() {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new() { TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString() };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());
        LogonResponse logonResponse = new LogonResponse { Balance = "0", Key = "Key", Msg = "Success", Status = 0 };

        var result = await this.PataPawaPrePayProxy.ProcessLogonMessage(CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.TransactionId.ShouldBe(operatorResponse.TransactionId);
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessLogonMessage_FailedLogon_MessageProcessed()
    {
        LogonResponse logonResponse = new LogonResponse { Balance = "0", Key = "Key", Msg = "Success", Status = -1 };

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(logonResponse));

        var result = await this.PataPawaPrePayProxy.ProcessLogonMessage(CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessLogonMessage_HttpCallFailed_MessageProcessed()
    {
        LogonResponse logonResponse = new LogonResponse { Balance = "0", Key = "Key", Msg = "Success", Status = -1 };

        this.MockHttpMessageHandler.When("http://localhost").Respond(req => new HttpResponseMessage(HttpStatusCode.BadRequest));

        var result = await this.PataPawaPrePayProxy.ProcessLogonMessage(CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_MeterTransaction_MessageProcessed()
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey", "APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "meter");
        metaDataDictionary.Add("MeterNumber", "123456");

        MeterResponse meterResponse = new MeterResponse { Status = 0, Code = "1", CustomerName = "Customer", Msg = "msg" };

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(meterResponse));
            
        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId, 
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);
            
        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe("0000");
        result.Data.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_MeterTransaction_FailedAtOperator_MessageProcessed()
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey", "APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "meter");
        metaDataDictionary.Add("MeterNumber", "123456");

        MeterResponse meterResponse = new MeterResponse { Status = -1, Code = "1", CustomerName = "Customer", Msg = "msg" };

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(meterResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_VendTransaction_MessageProcessed()
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey", "APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "vend");
        metaDataDictionary.Add("MeterNumber", "123456");
        metaDataDictionary.Add("CustomerName", "Mr Customer");
        metaDataDictionary.Add("Amount", "100");

        VendResponse vendResponse = new VendResponse { Status = 0, Msg = "msg",
            Transaction = new BusinessLogic.OperatorInterfaces.PataPawaPrePay.Transaction {
                CustomerName = "Mr Customer"
            }};

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(vendResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ResponseCode.ShouldBe("0000");
        result.Data.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_VendTransaction_FailedAtOperator_MessageProcessed()
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey", "APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "vend");
        metaDataDictionary.Add("MeterNumber", "123456");
        metaDataDictionary.Add("CustomerName", "Mr Customer");
        metaDataDictionary.Add("Amount", "100");

        VendResponse vendResponse = new VendResponse
        {
            Status = -1,
            Msg = "msg",
            Transaction = new BusinessLogic.OperatorInterfaces.PataPawaPrePay.Transaction
            {
                CustomerName = "Mr Customer"
            }
        };

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(vendResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_VendTransaction_CustomerNameIsNullOrEmpty_MessageProcessed(String customerName)
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey", "APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "vend");
        metaDataDictionary.Add("MeterNumber", "123456");
        metaDataDictionary.Add("CustomerName", customerName);
        metaDataDictionary.Add("Amount", "100");

        VendResponse vendResponse = new VendResponse
        {
            Status = -1,
            Msg = "msg",
            Transaction = new BusinessLogic.OperatorInterfaces.PataPawaPrePay.Transaction
            {
                CustomerName = "Mr Customer"
            }
        };

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(vendResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_VendTransaction_AmountIsNullOrEmpty_MessageProcessed(String amount)
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey", "APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "vend");
        metaDataDictionary.Add("MeterNumber", "123456");
        metaDataDictionary.Add("CustomerName", "Mr Customer");
        metaDataDictionary.Add("Amount", amount);

        VendResponse vendResponse = new VendResponse
        {
            Status = -1,
            Msg = "msg",
            Transaction = new BusinessLogic.OperatorInterfaces.PataPawaPrePay.Transaction
            {
                CustomerName = "Mr Customer"
            }
        };

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(vendResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_UnknownMessageType_MessageProcessed()
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey", "APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "unknown");
        metaDataDictionary.Add("MeterNumber", "123456");
        metaDataDictionary.Add("CustomerName", "Mr Customer");
        metaDataDictionary.Add("Amount", "100");

        VendResponse vendResponse = new VendResponse
        {
            Status = 0,
            Msg = "msg",
            Transaction = new BusinessLogic.OperatorInterfaces.PataPawaPrePay.Transaction
            {
                CustomerName = "Mr Customer"
            }
        };

        this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(vendResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_NoLogonResponse_MessageProcessed()
    {
        //BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        //{
        //    TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
        //    AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
        //        {"PataPawaPrePaidAPIKey", "APIKey"}
        //    }
        //};

        //this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "meter");
        metaDataDictionary.Add("MeterNumber", "123456");

        //MeterResponse meterResponse = new MeterResponse { Status = 0, Code = "1", CustomerName = "Customer", Msg = "msg" };

        //this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(meterResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_APIKeyNullOrEmpty_MessageProcessed(String apiKey)
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey",apiKey}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "meter");
        metaDataDictionary.Add("MeterNumber", "123456");

        //MeterResponse meterResponse = new MeterResponse { Status = 0, Code = "1", CustomerName = "Customer", Msg = "msg" };

        //this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(meterResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_MessageTypeNullOrEmpty_MessageProcessed(String messageType)
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey","APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", messageType);
        metaDataDictionary.Add("MeterNumber", "123456");

        //MeterResponse meterResponse = new MeterResponse { Status = 0, Code = "1", CustomerName = "Customer", Msg = "msg" };

        //this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(meterResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PataPawaPrePayProxy_ProcessSaleMessage_MeterNumberNullOrEmpty_MessageProcessed(String meterNumber)
    {
        BusinessLogic.OperatorInterfaces.OperatorResponse operatorResponse = new()
        {
            TransactionId = Guid.Parse("2D9D6BBA-BDF4-4248-9B27-6B68374AC037").ToString(),
            AdditionalTransactionResponseMetadata = new Dictionary<String, String>{
                {"PataPawaPrePaidAPIKey","APIKey"}
            }
        };

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, new MemoryCacheEntryOptions());

        Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();
        metaDataDictionary.Add("PataPawaPrePayMessageType", "meter");
        metaDataDictionary.Add("MeterNumber", meterNumber);

        //MeterResponse meterResponse = new MeterResponse { Status = 0, Code = "1", CustomerName = "Customer", Msg = "msg" };

        //this.MockHttpMessageHandler.When("http://localhost").Respond("application/json", JsonConvert.SerializeObject(meterResponse));

        var result = await this.PataPawaPrePayProxy.ProcessSaleMessage(TestData.TransactionId,
            TestData.OperatorId, TestData.Merchant, TestData.TransactionDateTime, TestData.TransactionReference,
            metaDataDictionary, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

}