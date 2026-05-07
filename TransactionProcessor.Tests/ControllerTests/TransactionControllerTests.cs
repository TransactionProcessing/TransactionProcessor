using Shared.Serialisation;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TransactionProcessor.Tests.ControllerTests {
    using Common;
    using DataTransferObjects;
    using Shouldly;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using TransactionProcessor.BusinessLogic.OperatorInterfaces.PataPawaPrePay;
    using Xunit;

    [Collection("TestCollection")]
    public class TransactionControllerTests : IClassFixture<TransactionProcessorWebFactory<Startup>> {
        #region Fields

        /// <summary>
        /// The web application factory
        /// </summary>
        private readonly TransactionProcessorWebFactory<Startup> WebApplicationFactory;

        #endregion

        #region Constructors

        public TransactionControllerTests(TransactionProcessorWebFactory<Startup> webApplicationFactory) {
            this.WebApplicationFactory = webApplicationFactory;
        }

        #endregion

        [Fact(Skip = "Incomplete")]
        public async Task TransactionController_POST_LogonTransaction_LogonTransactionResponseIsReturned() {
            HttpClient client = this.WebApplicationFactory.CreateClient();

            LogonTransactionRequest logonTransactionRequest = new LogonTransactionRequest();

            String uri = "api/transactions";
            StringContent content = Helpers.CreateStringContent(logonTransactionRequest);
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            HttpResponseMessage response = await client.PostAsync(uri, content, CancellationToken.None);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            String responseAsJson = await response.Content.ReadAsStringAsync();
            responseAsJson.ShouldNotBeNullOrEmpty();

            LogonTransactionResponse responseObject = StringSerialiser.Deserialise<LogonTransactionResponse>(responseAsJson);
            responseObject.ShouldNotBeNull();
            responseObject.ResponseCode.ShouldBe("0000");
            responseObject.ResponseMessage.ShouldBe("SUCCESS");
        }

        [Fact]
        public void Test() {
            var options = SystemTextJsonSerializer.GetDefaultJsonSerializerOptions();
            options.Converters.Add(new DateTimeSpaceConverter());
            options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
            StringSerialiser.Initialise(new SystemTextJsonSerializer(options));
            String responseContent = "{\r\n  \"transaction\": {\r\n    \"fixed\": [\r\n      {\r\n        \"ercCharge\": 3.19,\r\n        \"forexCharge\": 0.47,\r\n        \"fuelIndexCharge\": 2.47,\r\n        \"inflationAdjustment\": 0,\r\n        \"monthlyFC\": 13.27,\r\n        \"repCharge\": 1.39,\r\n        \"totalTax\": 15.21\r\n      }\r\n    ],\r\n    \"customerName\": \"Customer 1\",\r\n    \"date\": \"2026-05-07 06:03:18\",\r\n    \"meterNo\": \"00000001\",\r\n    \"msg\": null,\r\n    \"ref\": \"20260507060318986\",\r\n    \"rescode\": \"elec000\",\r\n    \"status\": 0,\r\n    \"stdTokenAmt\": 64.0,\r\n    \"stdTokenRctNum\": \"Ce001OVS3709952\",\r\n    \"stdTokenTax\": \"0\",\r\n    \"token\": \"63f9a9c47a7149e58a73f404222158cc\",\r\n    \"totalAmount\": \"400\",\r\n    \"transactionId\": 1,\r\n    \"units\": \"6.1\",\r\n    \"vendor\": \"support\"\r\n  },\r\n  \"msg\": \"success\",\r\n  \"status\": 0\r\n}";
            VendResponse vendResponse = StringSerialiser.Deserialise<VendResponse>(responseContent, new SerialiserOptions(SerialiserPropertyFormat.CamelCase));
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

