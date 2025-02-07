using TransactionProcessor.BusinessLogic.Common;
using TransactionProcessor.Database.Contexts;

namespace TransactionProcessor.IntegrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Client;
    using Ductus.FluentDocker.Builders;
    using EstateManagement.Client;
    using EventStore.Client;
    using global::Shared.IntegrationTesting;
    using Newtonsoft.Json;
    using SecurityService.Client;
    using Shouldly;
    using Retry = IntegrationTests.Retry;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.IntegrationTesting.DockerHelper" />
    public class DockerHelper : global::Shared.IntegrationTesting.DockerHelper
    {
        #region Fields
        public static String TestBankAccountNumber = "12345678";

        /// <summary>
        /// The test bank sort code
        /// </summary>
        public static String TestBankSortCode = "112233";

        public HttpClient TestHostHttpClient;

        /// <summary>
        /// The security service client
        /// </summary>
        public ISecurityServiceClient SecurityServiceClient;
        
        /// <summary>
        /// The transaction processor client
        /// </summary>
        public ITransactionProcessorClient TransactionProcessorClient;

        private readonly TestingContext TestingContext;

        public EventStoreProjectionManagementClient ProjectionManagementClient;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="testingContext">The testing context.</param>
        public DockerHelper() {
            this.TestingContext = new TestingContext();
        }

        #endregion

        #region Methods

        private async Task ConfigureTestBank(String sortCode,
                                             String accountNumber,
                                             String callbackUrl)
        {
            this.Trace(this.TestHostHttpClient.BaseAddress.ToString());

            var hostConfig = new
            {
                sort_code = sortCode,
                account_number = accountNumber,
                callback_url = callbackUrl
            };

            await Retry.For(async () =>
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/testbank/configuration");
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(hostConfig), Encoding.UTF8, "application/json");
                var responseMessage = await this.TestHostHttpClient.SendAsync(requestMessage);
                responseMessage.IsSuccessStatusCode.ShouldBeTrue();
            });
        }

        public override ContainerBuilder SetupTransactionProcessorContainer(){

            List<String> variables = new List<String>();
            variables.Add($"OperatorConfiguration:PataPawaPrePay:Url=http://{this.TestHostContainerName}:{DockerPorts.TestHostPort}/api/patapawaprepay");

            this.AdditionalVariables.Add(ContainerType.FileProcessor, variables);
            //this.SetAdditionalVariables(ContainerType.FileProcessor, variables);

            return base.SetupTransactionProcessorContainer();
        }

        public override async Task CreateSubscriptions(){
            List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new();
            subscriptions.AddRange(MessagingService.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());
            //var estateSubscriptions = EstateManagement.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions();
            //subscriptions.AddRange(estateSubscriptions.Where(e => e.streamName != "$ce-EstateAggregate"));
            subscriptions.AddRange(TransactionProcessor.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());

            foreach ((String streamName, String groupName, Int32 maxRetries) subscription in subscriptions)
            {
                var x = subscription;
                x.maxRetries = 2;
                await this.CreatePersistentSubscription(x);
            }
        }

        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices){
            
            await base.StartContainersForScenarioRun(scenarioName, dockerServices);
            
            // Setup the base address resolvers
            String EstateManagementBaseAddressResolver(String api) => $"http://127.0.0.1:{this.EstateManagementPort}";
            String SecurityServiceBaseAddressResolver(String api) => $"https://127.0.0.1:{this.SecurityServicePort}";
            String TransactionProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TransactionProcessorPort}";

            HttpClientHandler clientHandler = new HttpClientHandler
                                              {
                                                  ServerCertificateCustomValidationCallback = (message,
                                                                                               certificate2,
                                                                                               arg3,
                                                                                               arg4) =>
                                                                                              {
                                                                                                  return true;
                                                                                              }
                                              };
            HttpClient httpClient = new HttpClient(clientHandler);
            //this.EstateClient = new IntermediateEstateClient(new EstateClient(EstateManagementBaseAddressResolver, httpClient,2));
            this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient);
            this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient);
            this.TestHostHttpClient= new HttpClient(clientHandler);
            this.TestHostHttpClient.BaseAddress = new Uri($"http://127.0.0.1:{this.TestHostServicePort}");

            this.Trace("About to configure Test Bank");
            String callbackUrl = $"http://{this.CallbackHandlerContainerName}:{DockerPorts.CallbackHandlerDockerPort}/api/callbacks";
            await this.ConfigureTestBank(DockerHelper.TestBankSortCode, DockerHelper.TestBankAccountNumber, callbackUrl);
            this.Trace("Test Bank Configured");

            this.ProjectionManagementClient = new EventStoreProjectionManagementClient(ConfigureEventStoreSettings());
        }

        /// <summary>
        /// Stops the containers for scenario run.
        /// </summary>
        public override async Task StopContainersForScenarioRun(DockerServices dockerServices)
        {
            await this.RemoveEstateReadModel().ConfigureAwait(false);

            await base.StopContainersForScenarioRun(dockerServices);
        }
        
        private async Task RemoveEstateReadModel()
        {
            List<Guid> estateIdList = this.TestingContext.GetAllEstateIds();

            foreach (Guid estateId in estateIdList)
            {
                String databaseName = $"EstateReportingReadModel{estateId}";

                await Retry.For(async () =>
                                {
                                    // Build the connection string (to master)
                                    String connectionString = Setup.GetLocalConnectionString(databaseName);
                                    EstateManagementSqlServerContext context = new EstateManagementSqlServerContext(connectionString);
                                    await context.Database.EnsureDeletedAsync(CancellationToken.None);
                                });
            }
        }

        protected override List<String> GetRequiredProjections()
        {
            List<String> requiredProjections = new List<String>();

            requiredProjections.Add("CallbackHandlerEnricher.js");
            requiredProjections.Add("EstateAggregator.js");
            requiredProjections.Add("MerchantAggregator.js");
            requiredProjections.Add("MerchantBalanceCalculator.js");
            requiredProjections.Add("MerchantBalanceProjection.js");

            return requiredProjections;
        }
        
        #endregion
    }
}