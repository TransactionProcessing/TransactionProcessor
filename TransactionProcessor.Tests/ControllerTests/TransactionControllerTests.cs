using System;

namespace TransactionProcessor.Tests.ControllerTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using DataTransferObjects;
    using Newtonsoft.Json;
    using Shouldly;
    using Xunit;

    [Collection("TestCollection")]
    public class TransactionControllerTests : IClassFixture<TransactionProcessorWebFactory<Startup>>
    {
        #region Fields

        /// <summary>
        /// The web application factory
        /// </summary>
        private readonly TransactionProcessorWebFactory<Startup> WebApplicationFactory;

        #endregion

        #region Constructors

        public TransactionControllerTests(TransactionProcessorWebFactory<Startup> webApplicationFactory)
        {
            this.WebApplicationFactory = webApplicationFactory;
        }

        #endregion

        [Fact(Skip = "Incomplete")]
        public async Task TransactionController_POST_LogonTransaction_LogonTransactionResponseIsReturned()
        {
            HttpClient client = this.WebApplicationFactory.CreateClient();
            
            LogonTransactionRequest logonTransactionRequest = new LogonTransactionRequest();

            String uri = "api/transactions";
            StringContent content = Helpers.CreateStringContent(logonTransactionRequest);
            client.DefaultRequestHeaders.Add("api-version", "1.0");
            
            HttpResponseMessage response = await client.PostAsync(uri, content, CancellationToken.None);

            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            String responseAsJson = await response.Content.ReadAsStringAsync();
            responseAsJson.ShouldNotBeNullOrEmpty();

            LogonTransactionResponse responseObject = JsonConvert.DeserializeObject<LogonTransactionResponse>(responseAsJson);
            responseObject.ShouldNotBeNull();
            responseObject.ResponseCode.ShouldBe("0000");
            responseObject.ResponseMessage.ShouldBe("SUCCESS");
        }
    }
}
