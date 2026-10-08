using DotNet.Testcontainers.Builders;
using System.IO;
using TestHosts.Clients;
using TransactionProcessor.Database.Contexts;

namespace TransactionProcessor.IntegrationTests.Common
{
    using Client;
    using EventStore.Client;
    using global::Shared.IntegrationTesting;
    using global::Shared.IntegrationTesting.TestContainers;
    using global::Shared.Serialisation;
    using SecurityService.Client;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Retry = IntegrationTests.Retry;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.IntegrationTesting.DockerHelper" />
    public class DockerHelper : global::Shared.IntegrationTesting.TestContainers.DockerHelper
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

        public IAgencyBankingClient AgencyBankingClient;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="testingContext">The testing context.</param>
        public DockerHelper() {
            this.TestingContext = new TestingContext();
            StringSerialiser.Initialise((IStringSerialiser)new SystemTextJsonSerializer(SystemTextJsonSerializer.GetDefaultJsonSerializerOptions()));
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
                requestMessage.Content = new StringContent(StringSerialiser.Serialise(hostConfig), Encoding.UTF8, "application/json");
                var responseMessage = await this.TestHostHttpClient.SendAsync(requestMessage);
                responseMessage.IsSuccessStatusCode.ShouldBeTrue();
            });
        }

        //public override ContainerBuilder SetupTestHostContainer()
        //{
        //    var variables = new Dictionary<String, String>();
        //    variables.Add("ConnectionStrings:AgencyBankingReadModel", $"server={this.SqlServerContainerName};user id={this.SqlCredentials.usename};password={this.SqlCredentials.password};database=AgencyBankingReadModel;Encrypt=false");
        //    variables.Add("ASPNETCORE_ENVIRONMENT", $"PRODUCTION");
        //    this.AdditionalVariables.Add(ContainerType.TestHost, variables);

        //    return base.SetupTestHostContainer();
        //}

        public override ContainerBuilder SetupTransactionProcessorContainer() {
             Dictionary<String, String> additionalVariables = new();
            additionalVariables.Add("OperatorConfiguration:AgencyBanking:Url",$"http://{this.TestHostContainerName}:{DockerPorts.TestHostPort}/api/agencybanking");

            this.AdditionalVariables.Add(ContainerType.TransactionProcessor, additionalVariables);

            return base.SetupTransactionProcessorContainer();
        }

        public override ContainerBuilder SetupTestHostContainer()
        {
            this.Trace("About to Start Test Hosts Container");

            Dictionary<String, String> environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add("ConnectionStrings:TestBankReadModel", this.SetConnectionString("TestBankReadModel", this.UseSecureSqlServerDatabase));
            environmentVariables.Add("ConnectionStrings:PataPawaReadModel", this.SetConnectionString("PataPawaReadModel", this.UseSecureSqlServerDatabase));
            environmentVariables.Add("ConnectionStrings:AgencyBankingReadModel", this.SetConnectionString("AgencyBankingReadModel", this.UseSecureSqlServerDatabase));
            //environmentVariables.Add("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

            Dictionary<String, String> additionalEnvironmentVariables = this.GetAdditionalVariables(ContainerType.TestHost);

            foreach (KeyValuePair<String, String> additionalEnvironmentVariable in additionalEnvironmentVariables)
            {
                environmentVariables.Add(additionalEnvironmentVariable.Key, additionalEnvironmentVariable.Value);
            }

            (String imageName, Boolean useLatest) imageDetails = this.GetImageDetails(ContainerType.TestHost).Data;

            ContainerBuilder testHostContainer = new ContainerBuilder()
                .WithName(this.TestHostContainerName)  // similar to WithName()
                .WithImage(imageDetails.imageName)
                .WithEnvironment(environmentVariables)
                .MountHostFolder(this.DockerPlatform, this.HostTraceFolder)
                .WithPortBinding(DockerPorts.TestHostPort, true);

            return testHostContainer;
        }

        public override async Task CreateSubscriptions(){
            List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new();
            subscriptions.AddRange(MessagingService.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());
            subscriptions.AddRange(TransactionProcessor.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());
            foreach ((String streamName, String groupName, Int32 maxRetries) subscription in subscriptions)
            {
                var x = subscription;
                x.maxRetries = 2;
                await this.CreatePersistentSubscription(x);
            }
        }

        String Serialise(Object arg)
        {
            return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        Object Deserialise(String arg, Type type)
        {
            return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        String Serialise_CamelCase(Object arg)
        {
            return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.CamelCase));
        }

        Object Deserialise_CamelCase(String arg, Type type)
        {
            return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.CamelCase));
        }

        public String AccessToken;

        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices){
            this.HostTraceFolder = Path.Combine(Directory.GetCurrentDirectory(), "trace");
            await base.StartContainersForScenarioRun(scenarioName, dockerServices);
            
            // Setup the base address resolvers
            String SecurityServiceBaseAddressResolver(String api) => $"https://127.0.0.1:{this.SecurityServicePort}";
            String TransactionProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TransactionProcessorPort}";
            String TestHostServiceBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TestHostServicePort}";

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
            HttpClient httpClient = new(clientHandler);
            this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient, Serialise, Deserialise);
            this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient, Serialise, Deserialise);
            this.TestHostHttpClient= new HttpClient(clientHandler);
            this.TestHostHttpClient.BaseAddress = new Uri($"http://127.0.0.1:{this.TestHostServicePort}");
            this.AgencyBankingClient = new AgencyBankingClient(TestHostServiceBaseAddressResolver, httpClient, Serialise_CamelCase, this.Deserialise_CamelCase);

            this.Trace("About to configure Test Bank");
            String callbackUrl = $"http://{this.CallbackHandlerContainerName}:{DockerPorts.CallbackHandlerDockerPort}/api/callbacks";
            await this.ConfigureTestBank(DockerHelper.TestBankSortCode, DockerHelper.TestBankAccountNumber, callbackUrl);
            this.Trace("Test Bank Configured");

            this.ProjectionManagementClient = new EventStoreProjectionManagementClient(ConfigureEventStoreSettings());

            SimpleResults.Result<SecurityService.DataTransferObjects.TokenResponse> bootstrapToken = await this.SecurityServiceClient.GetToken("management-bootstrap", "management-bootstrap-secret", CancellationToken.None);
            if (bootstrapToken.IsFailed || String.IsNullOrWhiteSpace(bootstrapToken.Data?.AccessToken))
            {
                throw new InvalidOperationException("Unable to obtain the integration-test management bootstrap token.");
            }
            this.AccessToken = bootstrapToken.Data.AccessToken;
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
            //List<Guid> estateIdList = this.TestingContext.GetAllEstateIds();

            //foreach (Guid estateId in estateIdList)
            //{
            //    String databaseName = $"EstateReportingReadModel{estateId}";

            //    await Retry.For(async () =>
            //                    {
            //                        // Build the connection string (to master)
            //                        String connectionString = Setup.GetLocalConnectionString(databaseName);
            //                        EstateManagementContext context = new EstateManagementContext(connectionString);
            //                        await context.Database.EnsureDeletedAsync(CancellationToken.None);
            //                    });
            //}
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
