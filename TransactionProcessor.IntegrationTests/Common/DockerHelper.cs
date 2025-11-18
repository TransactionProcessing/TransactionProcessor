using Ductus.FluentDocker.Model.Containers;
using Ductus.FluentDocker.Services;
using TransactionProcessor.Database.Contexts;

namespace TransactionProcessor.IntegrationTests.Common
{
    using Client;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Executors;
    using Ductus.FluentDocker.Services.Extensions;
    using EventStore.Client;
    using global::Shared.IntegrationTesting;
    using Newtonsoft.Json;
    using SecurityService.Client;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Retry = IntegrationTests.Retry;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.IntegrationTesting.DockerHelper" />
    public class DockerHelper : global::Shared.IntegrationTesting.DockerHelper
    {

        public override INetworkService SetupTestNetwork(String networkName = null,
                                                        Boolean reuseIfExists = false)
        {
            networkName = String.IsNullOrEmpty(networkName) ? $"testnw{this.TestId:N}" : networkName;
            //SimpleResults.Result<DockerEnginePlatform> engineType = BaseDockerHelper.GetDockerEnginePlatform();
            //Console.WriteLine($"Engine Type is {engineType.Data}");

            // Build a network
            NetworkBuilder networkService = new Builder().UseNetwork(networkName).ReuseIfExist();

            return networkService.Build();
        }

        public override async Task<IContainerService> SetupSqlServerContainer(INetworkService networkService)
        {
            if (this.SqlCredentials == default)
                throw new ArgumentNullException("Sql Credentials have not been set");

            IContainerService databaseServerContainer = await this.StartContainerX(this.ConfigureSqlContainer,
                new List<INetworkService>{
                    networkService
                },
                DockerServices.SqlServer);

            return databaseServerContainer;
        }

        protected async Task<IContainerService> StartContainerX(Func<ContainerBuilder> buildContainerFunc, List<INetworkService> networkServices, DockerServices dockerService)
        {
            if ((this.RequiredDockerServices & dockerService) != dockerService)
            {
                return default;
            }

            //ConsoleStream<String> consoleLogs = null;
            try {
                SimpleResults.Result<DockerEnginePlatform> dockerEnginePlatform = BaseDockerHelper.GetDockerEnginePlatform();
                Console.WriteLine($"Engine Type is {dockerEnginePlatform.Data}");

                this.Trace($"{dockerService} about to call builder func");
                ContainerBuilder containerBuilder = buildContainerFunc();

                this.Trace($"{dockerService} about to call build");
                IContainerService builtContainer = containerBuilder.Build();

                this.Trace($"{dockerService} about to attach logs");
                //consoleLogs = builtContainer.Logs(true);

                this.Trace($"{dockerService} about to call Start");
                IContainerService startedContainer = builtContainer.Start();
                if (startedContainer == null)
                    throw new Exception($"{dockerService} startedContainer is null");
                this.Trace($"{dockerService} after call to Start");
                if (networkServices == null || networkServices.Count == 0) {
                    this.Trace($"{dockerService} No network services to attach");
                }
                else{
                    foreach (INetworkService networkService in networkServices) {
                        this.Trace($"{dockerService} about to attach network service");
                        if (networkService == null)
                            throw new ArgumentNullException("networkService is null");

                        networkService.Attach(startedContainer, false);
                    }
            }

            this.Trace($"{dockerService} Container Started");
                this.Containers.Add((dockerService, startedContainer));
                
                await DoSqlServerHealthCheck(startedContainer);
                
                this.Trace($"Container [{buildContainerFunc.Method.Name}] started");

                return startedContainer;
            }
            catch (Exception ex)
            {
                //if (consoleLogs != null)
                //{
                //    while (!consoleLogs.IsFinished)
                //    {
                //        String s = consoleLogs.TryRead(10000);
                //        this.Trace(s);
                //    }
                //}

                this.Error($"Error starting container [{buildContainerFunc.Method.Name}]", ex);
                throw;
            }
        }


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
            subscriptions.AddRange(TransactionProcessor.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());
            foreach ((String streamName, String groupName, Int32 maxRetries) subscription in subscriptions)
            {
                var x = subscription;
                x.maxRetries = 2;
                await this.CreatePersistentSubscription(x);
            }
        }

        public override ContainerBuilder SetupSecurityServiceContainer()
        {
            this.Trace("About to Start Security Container");

            List<String> environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add($"ServiceOptions:PublicOrigin=https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            environmentVariables.Add($"ServiceOptions:IssuerUrl=https://{this.SecurityServiceContainerName}:{DockerPorts.SecurityServiceDockerPort}");
            //environmentVariables.Add("ASPNETCORE_ENVIRONMENT=IntegrationTest");
            environmentVariables.Add($"urls=https://*:{DockerPorts.SecurityServiceDockerPort}");

            environmentVariables.Add("ServiceOptions:PasswordOptions:RequiredLength=6");
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequireDigit=false");
            environmentVariables.Add("ServiceOptions:PasswordOptions:RequireUpperCase=false");
            environmentVariables.Add("ServiceOptions:UserOptions:RequireUniqueEmail=false");
            environmentVariables.Add("ServiceOptions:SignInOptions:RequireConfirmedEmail=false");

            environmentVariables.Add(this.SetConnectionString("ConnectionStrings:PersistedGrantDbContext", $"PersistedGrantStore-{this.TestId}", this.UseSecureSqlServerDatabase));
            environmentVariables.Add(this.SetConnectionString("ConnectionStrings:ConfigurationDbContext", $"Configuration-{this.TestId}", this.UseSecureSqlServerDatabase));
            environmentVariables.Add(this.SetConnectionString("ConnectionStrings:AuthenticationDbContext", $"Authentication-{this.TestId}", this.UseSecureSqlServerDatabase));

            List<String> additionalEnvironmentVariables = this.GetAdditionalVariables(ContainerType.SecurityService);

            if (additionalEnvironmentVariables != null)
            {
                environmentVariables.AddRange(additionalEnvironmentVariables);
            }

            var imageDetails = this.GetImageDetails(ContainerType.SecurityService);
            if (imageDetails.IsFailed)
                throw new Exception(imageDetails.Message);
            ContainerBuilder securityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                                     .WithEnvironment(environmentVariables.ToArray())
                                                                     .UseImageDetails(imageDetails.Data)
                                                                     .MountHostFolder(this.DockerPlatform, this.HostTraceFolder)
                                                                     .SetDockerCredentials(this.DockerCredentials);

            Int32? hostPort = this.GetHostPort(ContainerType.SecurityService);
            if (hostPort == null)
            {
                securityServiceContainer = securityServiceContainer.ExposePort(DockerPorts.SecurityServiceDockerPort);
            }
            else
            {
                securityServiceContainer = securityServiceContainer.ExposePort(hostPort.Value, DockerPorts.SecurityServiceDockerPort);
            }

            // Now build and return the container                
            return securityServiceContainer;
        }

        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices){
            
            await base.StartContainersForScenarioRun(scenarioName, dockerServices);
            
            // Setup the base address resolvers
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

            await this.StopContainersForScenarioRunX(dockerServices);
        }

        public async Task StopContainersForScenarioRunX(DockerServices sharedDockerServices)
        {
            if (this.Containers.Any())
            {
                this.Containers.Reverse();

                foreach ((DockerServices, IContainerService) containerService in this.Containers)
                {

                    if ((sharedDockerServices & containerService.Item1) == containerService.Item1)
                    {
                        continue;
                    }

                    this.Trace($"Stopping container [{containerService.Item2.Name}]");
                    if (containerService.Item2.Name.Contains("eventstore"))
                    {
                        CopyEventStoreLogs(containerService.Item2);
                    }

                    containerService.Item2.Stop();
                    containerService.Item2.Remove(true);
                    this.Trace($"Container [{containerService.Item2.Name}] stopped");
                }
            }

            if (this.TestNetworks.Any())
            {
                foreach (INetworkService networkService in this.TestNetworks)
                {
                    this.Trace($"Teardown network {networkService.Name}");
                    var cfg = networkService.GetConfiguration(true);
                    if (!cfg.Containers.Any())
                    {
                        networkService.Stop();
                        networkService.Remove(true);
                    }
                }
            }
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
                                    EstateManagementContext context = new EstateManagementContext(connectionString);
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