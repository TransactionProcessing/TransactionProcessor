using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common;
using EstateManagement.DataTransferObjects.Responses.Merchant;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Shared.Logger;

public class PataPawaPrePayProxy : IOperatorProxy{
    private readonly PataPawaPrePaidConfiguration Configuration;

    private readonly HttpClient HttpClient;

    private readonly IMemoryCache MemoryCache;

    public PataPawaPrePayProxy(PataPawaPrePaidConfiguration configuration,
                               HttpClient httpClient,
                               IMemoryCache memoryCache){
        this.Configuration = configuration;
        this.HttpClient = httpClient;
        this.MemoryCache = memoryCache;
    }

    public async Task<Result<OperatorResponse>> ProcessLogonMessage(String accessToken, CancellationToken cancellationToken){
        // Check if we need to do a logon with the operator
        OperatorResponse operatorResponse = this.MemoryCache.Get<OperatorResponse>("PataPawaPrePayLogon");
        if (operatorResponse != null){
            return Result.Success(operatorResponse);
        }

        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, this.Configuration.Url);
        MultipartFormDataContent content = new MultipartFormDataContent();
        content.Add(new StringContent("login"), "request");
        content.Add(new StringContent(this.Configuration.Username), "username");
        content.Add(new StringContent(this.Configuration.Password), "password");
        requestMessage.Content = content;

        HttpResponseMessage responseMessage = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

        // Check the send was successful
        if (responseMessage.IsSuccessStatusCode == false){
            return Result.Failure($"Error sending logon request to Patapawa. Status Code [{responseMessage.StatusCode}]");
        }

        // Get the response
        String responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        Logger.LogInformation($"Received response message from Patapawa [{responseContent}]");

        return this.CreateFromLogon(responseContent);
    }

    public async Task<Result<OperatorResponse>> ProcessSaleMessage(String accessToken, Guid transactionId, Guid operatorId,
                                                                   MerchantResponse merchant, DateTime transactionDateTime, String transactionReference, Dictionary<String, String> additionalTransactionMetadata, CancellationToken cancellationToken){
        // Get the logon response for the operator
        OperatorResponse logonResponse = this.MemoryCache.Get<OperatorResponse>("PataPawaPrePayLogon");
        if (logonResponse == null)
        {
            return Result.Invalid("PataPawaPrePayLogon - logonResponse is null");
        }
        String apiKey = logonResponse.AdditionalTransactionResponseMetadata.ExtractFieldFromMetadata<String>("PataPawaPrePaidAPIKey");

        if (String.IsNullOrEmpty(apiKey))
        {
            return Result.Invalid("PataPawaPrePayAPIKey - APIKey is a required field for this transaction type");
        }

        // Check the meta data for the sale message type
        String messageType = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("PataPawaPrePayMessageType");

        if (String.IsNullOrEmpty(messageType))
        {
            return Result.Invalid("PataPawaPrePayMessageType - Message Type is a required field for this transaction type");
        }

        String meterNumber = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("MeterNumber");

        if (String.IsNullOrEmpty(meterNumber))
        {
            return Result.Invalid("MeterNumber - Meter Number is a required field for this transaction type");
        }

        return messageType switch
        {
            "meter" => await this.PerformMeterTransaction(meterNumber, apiKey, cancellationToken),
            "vend" => await this.PerformVendTransaction(meterNumber, apiKey, additionalTransactionMetadata, cancellationToken),
            _ => Result.Invalid($"PataPawaPrePayMessageType - Unsupported Message Type {messageType}")
        };
    }

    private async Task<Result<OperatorResponse>> PerformMeterTransaction(String meterNumber, String apiKey, CancellationToken cancellationToken){
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, this.Configuration.Url);
        MultipartFormDataContent content = new MultipartFormDataContent();
        content.Add(new StringContent("meter"), "request");
        content.Add(new StringContent(this.Configuration.Username), "username");
        content.Add(new StringContent(meterNumber), "meter");
        content.Add(new StringContent(apiKey), "key");
        requestMessage.Content = content;

        HttpResponseMessage responseMessage = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

        // Get the response
        String responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        Logger.LogInformation($"Received response message from Patapawa [{responseContent}]");

        return this.CreateFromMeter(responseContent);
    }

    private async Task<Result<OperatorResponse>> PerformVendTransaction(String meterNumber, String apiKey, Dictionary<String, String> additionalTransactionMetadata, CancellationToken cancellationToken)
    {
        String customerName = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("CustomerName");

        if (String.IsNullOrEmpty(customerName))
        {
            return Result.Invalid("CustomerName - Customer Name is a required field for this transaction type");
        }

        String amount = additionalTransactionMetadata.ExtractFieldFromMetadata<String>("Amount");

        if (String.IsNullOrEmpty(amount))
        {
            return Result.Invalid("Amount - Amount is a required field for this transaction type");
        }


        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, this.Configuration.Url);
        MultipartFormDataContent content = new MultipartFormDataContent();
        content.Add(new StringContent("vend"), "request");
        content.Add(new StringContent(this.Configuration.Username), "username");
        content.Add(new StringContent(meterNumber), "meter");
        content.Add(new StringContent(apiKey), "key");
        content.Add(new StringContent(amount), "amount");
        content.Add(new StringContent(customerName), "customerName");
        requestMessage.Content = content;

        HttpResponseMessage responseMessage = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

        // Get the response
        String responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        Logger.LogInformation($"Received response message from Patapawa [{responseContent}]");

        return this.CreateFromVend(responseContent);
    }

    private Result<OperatorResponse> CreateFromLogon(String responseContent){
        LogonResponse logonResponse = JsonConvert.DeserializeObject<LogonResponse>(responseContent);

        if (logonResponse.Status != 0) {
            return Result.Failure($"Error logging on with PataPawa Pre Paid API, Response is {logonResponse.Status}");
        }

        OperatorResponse operatorResponse = new OperatorResponse{
                                                                    IsSuccessful = true,
                                                                    ResponseCode = "0000",
                                                                    ResponseMessage = logonResponse.Msg,
                                                                    AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
                                                                };

        operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaPrePaidAPIKey", logonResponse.Key);
        operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaprePaidBalance", logonResponse.Balance.ToString());

        this.MemoryCache.Set("PataPawaPrePayLogon", operatorResponse, this.MemoryCacheEntryOptions);

        return Result.Success(operatorResponse);
    }

    private Result<OperatorResponse> CreateFromMeter(String responseContent)
    {
        MeterResponse meterResponse = JsonConvert.DeserializeObject<MeterResponse>(responseContent);

        if (meterResponse.Status != 0)
        {
            return Result.Failure("Error during meter transaction");
        }

        OperatorResponse operatorResponse = new OperatorResponse
                                            {
                                                IsSuccessful = true,
                                                ResponseCode = "0000",
                                                ResponseMessage = meterResponse.Msg,
                                                AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
                                            };

        operatorResponse.AdditionalTransactionResponseMetadata.Add("PataPawaPrePaidCustomerName", meterResponse.CustomerName);

        return Result.Success(operatorResponse);
    }

    private Result<OperatorResponse> CreateFromVend(String responseContent)
    {
        VendResponse vendResponse = JsonConvert.DeserializeObject<VendResponse>(responseContent);

        if (vendResponse.Status != 0)
        {
            return Result.Failure("Error during vend transaction");
        }

        OperatorResponse operatorResponse = new OperatorResponse
                                            {
                                                IsSuccessful = true,
                                                ResponseCode = "0000",
                                                ResponseMessage = vendResponse.Msg,
                                                AdditionalTransactionResponseMetadata = new Dictionary<String, String>()
                                            };

        operatorResponse.AdditionalTransactionResponseMetadata.Add("TransactionId", vendResponse.Transaction.TransactionId.ToString());
        operatorResponse.AdditionalTransactionResponseMetadata.Add("Reference", vendResponse.Transaction.Ref);
        operatorResponse.AdditionalTransactionResponseMetadata.Add("Units", vendResponse.Transaction.Units);
        operatorResponse.AdditionalTransactionResponseMetadata.Add("Token", vendResponse.Transaction.Token);
        operatorResponse.AdditionalTransactionResponseMetadata.Add("ReceiptNumber", vendResponse.Transaction.StdTokenRctNum);
        operatorResponse.AdditionalTransactionResponseMetadata.Add("TokenAmount", vendResponse.Transaction.StdTokenAmt.ToString());
        operatorResponse.AdditionalTransactionResponseMetadata.Add("TokenTax", vendResponse.Transaction.StdTokenTax.ToString());
        operatorResponse.AdditionalTransactionResponseMetadata.Add("MeterNumber", vendResponse.Transaction.MeterNo);
        operatorResponse.AdditionalTransactionResponseMetadata.Add("TransactionDateTime", vendResponse.Transaction.Date.ToString("yyyy-MM-dd HH:mm:ss"));
        operatorResponse.AdditionalTransactionResponseMetadata.Add("CustomerName", vendResponse.Transaction.CustomerName);
        
        return Result.Success(operatorResponse);
    }


    private MemoryCacheEntryOptions MemoryCacheEntryOptions =>
        new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove).SetSlidingExpiration(TimeSpan.FromHours(1))
                                     .RegisterPostEvictionCallback(this.PostEvictionCallback);

    [ExcludeFromCodeCoverage]
    private void PostEvictionCallback(Object key,
                                      Object value,
                                      EvictionReason reason,
                                      Object state){
        if (key.ToString().Contains("Logon")){
            this.ProcessLogonMessage(String.Empty, CancellationToken.None).Wait();
        }
    }
}