using Shared.Serialisation;
using SimpleResults;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;

using Common;
using Microsoft.Extensions.Caching.Memory;
using Shared.Logger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

    public async Task<Result<OperatorResponse>> ProcessLogonMessage(CancellationToken cancellationToken){
        try {
            // Check if we need to do a logon with the operator
            OperatorResponse operatorResponse = this.MemoryCache.Get<OperatorResponse>("PataPawaPrePayLogon");
            if (operatorResponse != null) {
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
            if (responseMessage.IsSuccessStatusCode == false) {
                return Result.Failure($"Error sending logon request to Patapawa. Status Code [{responseMessage.StatusCode}]");
            }

            // Get the response
            String responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

            Logger.LogInformation($"Received response message from Patapawa [{responseContent}]");

            return this.CreateFromLogon(responseContent);
        }
        catch (Exception ex) {
            return Result.Failure($"Error processing logon message for PataPawaPrePay [{ex.Message}]");
        }
    }

    public async Task<Result<OperatorResponse>> ProcessSaleMessage(Guid transactionId, Guid operatorId,
                                                                   Models.Merchant.Merchant merchant, DateTime transactionDateTime, String transactionReference, Dictionary<String, String> additionalTransactionMetadata, CancellationToken cancellationToken){
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
        LogonResponse logonResponse = StringSerialiser.Deserialise<LogonResponse>(responseContent);

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
        MeterResponse meterResponse = StringSerialiser.Deserialise<MeterResponse>(responseContent, new SerialiserOptions(SerialiserPropertyFormat.CamelCase));

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
        VendResponse vendResponse = StringSerialiser.Deserialise<VendResponse>(responseContent);

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
            this.ProcessLogonMessage(CancellationToken.None).Wait();
        }
    }

    public class DateTimeSpaceConverter : JsonConverter<DateTime>
    {
        private static readonly string[] AcceptedFormats = new[] {
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd H:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", "o" // ISO 8601 round-trip
                                                                                                                                                                                };
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return default;

                // Try exact known formats first (handles "2026-05-07 06:03:18")
                if (DateTime.TryParseExact(s, AcceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var dtExact))
                    return dtExact;

                // Fall back to general parse
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
                    return dt;

                throw new JsonException($"Unable to parse DateTime: '{s}'.");
            }

            // If JSON contains a number, attempt to treat it as Unix seconds (optional)
            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out long seconds))
            {
                return DateTimeOffset.FromUnixTimeSeconds(seconds).LocalDateTime;
            }

            throw new JsonException($"Unexpected token parsing DateTime. Token: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Write in the same "space" format so round-trip matches your input
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}