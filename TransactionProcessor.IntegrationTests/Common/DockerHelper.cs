namespace TransactionProcessor.IntegrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Client;
    using Ductus.FluentDocker.Common;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using EstateManagement.Client;
    using EstateReporting.Database;
    using global::Shared.Logger;
    using Microsoft.Data.SqlClient;
    using SecurityService.Client;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.IntegrationTesting.DockerHelper" />
    public class DockerHelper : global::Shared.IntegrationTesting.DockerHelper
    {
        #region Fields

        /// <summary>
        /// The estate client
        /// </summary>
        public IEstateClient EstateClient;

        /// <summary>
        /// The security service client
        /// </summary>
        public ISecurityServiceClient SecurityServiceClient;

        /// <summary>
        /// The test identifier
        /// </summary>
        public Guid TestId;

        /// <summary>
        /// The transaction processor client
        /// </summary>
        public ITransactionProcessorClient TransactionProcessorClient;

        /// <summary>
        /// The containers
        /// </summary>
        protected List<IContainerService> Containers;

        /// <summary>
        /// The estate management API port
        /// </summary>
        protected Int32 EstateManagementApiPort;

        /// <summary>
        /// The event store HTTP port
        /// </summary>
        protected Int32 EventStoreHttpPort;

        /// <summary>
        /// The security service port
        /// </summary>
        protected Int32 SecurityServicePort;

        /// <summary>
        /// The test networks
        /// </summary>
        protected List<INetworkService> TestNetworks;

        protected String SecurityServiceContainerName;

        protected String EstateManagementContainerName;

        protected String EventStoreContainerName;

        protected String EstateReportingContainerName;

        protected String SubscriptionServiceContainerName;

        protected String TransactionProcessorContainerName;

        /// <summary>
        /// The transaction processor port
        /// </summary>
        protected Int32 TransactionProcessorPort;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly NlogLogger Logger;

        private readonly TestingContext TestingContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="testingContext">The testing context.</param>
        public DockerHelper(NlogLogger logger, TestingContext testingContext)
        {
            this.Logger = logger;
            this.TestingContext = testingContext;
            this.Containers = new List<IContainerService>();
            this.TestNetworks = new List<INetworkService>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = FdOs.IsWindows() ? $"D:\\home\\txnproc\\trace\\{scenarioName}" : $"//home//txnproc//trace//{scenarioName}";

            Logging.Enabled();

            Guid testGuid = Guid.NewGuid();
            this.TestId = testGuid;

            this.Logger.LogInformation($"Test Id is {testGuid}");
            
            // Setup the container names
            this.SecurityServiceContainerName = $"securityservice{testGuid:N}";
            this.EstateManagementContainerName = $"estate{testGuid:N}";
            this.EventStoreContainerName = $"eventstore{testGuid:N}";
            this.EstateReportingContainerName = $"estatereporting{testGuid:N}";
            this.SubscriptionServiceContainerName = $"subscription{testGuid:N}";
            this.TransactionProcessorContainerName = $"txnprocessor{testGuid:N}";

            (String, String, String) dockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

            INetworkService testNetwork = DockerHelper.SetupTestNetwork();
            this.TestNetworks.Add(testNetwork);
            IContainerService eventStoreContainer =
                DockerHelper.SetupEventStoreContainer(this.EventStoreContainerName, this.Logger, "eventstore/eventstore:release-5.0.2", testNetwork, traceFolder);

            IContainerService estateManagementContainer = DockerHelper.SetupEstateManagementContainer(this.EstateManagementContainerName, this.Logger,
                                                                                                      "stuartferguson/estatemanagement", new List<INetworkService>
                                                                                                                          {
                                                                                                                              testNetwork,
                                                                                                                              Setup.DatabaseServerNetwork
                                                                                                                          }, traceFolder, null,
                                                                                                      this.SecurityServiceContainerName,
                                                                                                      this.EventStoreContainerName,
                                                                                                      Setup.SqlServerContainerName,
                                                                                                      "sa",
                                                                                                      "thisisalongpassword123!",
                                                                                                      ("serviceClient", "Secret1"));

            IContainerService securityServiceContainer = DockerHelper.SetupSecurityServiceContainer(this.SecurityServiceContainerName,
                                                                                                    this.Logger,
                                                                                                    "stuartferguson/securityservice",
                                                                                                    testNetwork,
                                                                                                    traceFolder,
                                                                                                    dockerCredentials,
                                                                                                    true);

            IContainerService transactionProcessorContainer = DockerHelper.SetupTransactionProcessorContainer(this.TransactionProcessorContainerName,
                                                                                                              this.Logger,
                                                                                                              "transactionprocessor",
                                                                                                              new List<INetworkService>
                                                                                                              {
                                                                                                                  testNetwork
                                                                                                              },
                                                                                                              traceFolder,
                                                                                                              dockerCredentials,
                                                                                                              this.SecurityServiceContainerName,
                                                                                                              this.EstateManagementContainerName,
                                                                                                              this.EventStoreContainerName,
                                                                                                              ("serviceClient", "Secret1"));

            IContainerService estateReportingContainer = DockerHelper.SetupEstateReportingContainer(this.EstateReportingContainerName,
                                                                                                    this.Logger,
                                                                                                    "stuartferguson/estatereporting",
                                                                                                    new List<INetworkService>
                                                                                                    {
                                                                                                        testNetwork,
                                                                                                        Setup.DatabaseServerNetwork
                                                                                                    },
                                                                                                    traceFolder,
                                                                                                    dockerCredentials,
                                                                                                    this.SecurityServiceContainerName,
                                                                                                    Setup.SqlServerContainerName,
                                                                                                    "sa",
                                                                                                    "thisisalongpassword123!",
                                                                                                    ("serviceClient", "Secret1"),
                                                                                                    true);

            this.Containers.AddRange(new List<IContainerService>
                                     {
                                         eventStoreContainer,
                                         estateManagementContainer,
                                         securityServiceContainer,
                                         transactionProcessorContainer,
                                         estateReportingContainer
                                     });

            // Cache the ports
            this.EstateManagementApiPort = estateManagementContainer.ToHostExposedEndpoint("5000/tcp").Port;
            this.SecurityServicePort = securityServiceContainer.ToHostExposedEndpoint("5001/tcp").Port;
            this.EventStoreHttpPort = eventStoreContainer.ToHostExposedEndpoint("2113/tcp").Port;
            this.TransactionProcessorPort = transactionProcessorContainer.ToHostExposedEndpoint("5002/tcp").Port;

            // Setup the base address resolvers
            String EstateManagementBaseAddressResolver(String api) => $"http://127.0.0.1:{this.EstateManagementApiPort}";
            String SecurityServiceBaseAddressResolver(String api) => $"http://127.0.0.1:{this.SecurityServicePort}";
            String TransactionProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TransactionProcessorPort}";

            HttpClient httpClient = new HttpClient();
            this.EstateClient = new EstateClient(EstateManagementBaseAddressResolver, httpClient);
            this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient);
            this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient);

            await PopulateSubscriptionServiceConfiguration().ConfigureAwait(false);

            IContainerService subscriptionServiceContainer = DockerHelper.SetupSubscriptionServiceContainer(this.SubscriptionServiceContainerName,
                                                                                                            this.Logger,
                                                                                                            "stuartferguson/subscriptionservicehost",
                                                                                                            new List<INetworkService>
                                                                                                            {
                                                                                                                testNetwork,
                                                                                                                Setup.DatabaseServerNetwork
                                                                                                            },
                                                                                                            traceFolder,
                                                                                                            dockerCredentials,
                                                                                                            this.SecurityServiceContainerName,
                                                                                                            Setup.SqlServerContainerName,
                                                                                                            "sa",
                                                                                                            "thisisalongpassword123!",
                                                                                                            this.TestId,
                                                                                                            ("serviceClient", "Secret1"),
                                                                                                            true);

            this.Containers.Add(subscriptionServiceContainer);
        }

        protected async Task PopulateSubscriptionServiceConfiguration()
        {
            String connectionString = Setup.GetLocalConnectionString("SubscriptionServiceConfiguration");

            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync(CancellationToken.None).ConfigureAwait(false);

                    // Create an Event Store Server
                    await this.InsertEventStoreServer(connection, this.EventStoreContainerName).ConfigureAwait(false);

                    String endPointUri = $"http://{this.EstateReportingContainerName}:5005/api/domainevents";
                    // Add Route for Estate Aggregate Events
                    await this.InsertSubscription(connection, "$ce-EstateAggregate", "Reporting", endPointUri).ConfigureAwait(false);

                    // Add Route for Merchant Aggregate Events
                    await this.InsertSubscription(connection, "$ce-MerchantAggregate", "Reporting", endPointUri).ConfigureAwait(false);

                    await connection.CloseAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }
        protected async Task CleanUpSubscriptionServiceConfiguration()
        {
            String connectionString = Setup.GetLocalConnectionString("SubscriptionServiceConfiguration");

            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(CancellationToken.None).ConfigureAwait(false);

                // Delete the Event Store Server
                await this.DeleteEventStoreServer(connection).ConfigureAwait(false);

                // Delete the Subscriptions
                await this.DeleteSubscriptions(connection).ConfigureAwait(false);

                await connection.CloseAsync().ConfigureAwait(false);
            }
        }

        protected async Task InsertEventStoreServer(SqlConnection openConnection, String eventStoreContainerName)
        {
            String esConnectionString = $"ConnectTo=tcp://admin:changeit@{eventStoreContainerName}:{DockerHelper.EventStoreTcpDockerPort};VerboseLogging=true;";
            SqlCommand command = openConnection.CreateCommand();
            command.CommandText = $"INSERT INTO EventStoreServer(EventStoreServerId, ConnectionString,Name) SELECT '{this.TestId}', '{esConnectionString}', 'TestEventStore'";
            command.CommandType = CommandType.Text;
            await command.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false);
        }

        protected async Task DeleteEventStoreServer(SqlConnection openConnection)
        {
            SqlCommand command = openConnection.CreateCommand();
            command.CommandText = $"DELETE FROM EventStoreServer WHERE EventStoreServerId = '{this.TestId}'";
            command.CommandType = CommandType.Text;
            await command.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false);
        }

        protected async Task DeleteSubscriptions(SqlConnection openConnection)
        {
            SqlCommand command = openConnection.CreateCommand();
            command.CommandText = $"DELETE FROM Subscription WHERE EventStoreId = '{this.TestId}'";
            command.CommandType = CommandType.Text;
            await command.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false);
        }

        protected async Task InsertSubscription(SqlConnection openConnection, String streamName, String groupName, String endPointUri)
        {
            SqlCommand command = openConnection.CreateCommand();
            command.CommandText = $"INSERT INTO subscription(SubscriptionId, EventStoreId, StreamName, GroupName, EndPointUri, StreamPosition) SELECT '{Guid.NewGuid()}', '{this.TestId}', '{streamName}', '{groupName}', '{endPointUri}', null";
            command.CommandType = CommandType.Text;
            await command.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false);
        }

        private async Task RemoveEstateReadModel()
        {
            List<Guid> estateIdList = this.TestingContext.GetAllEstateIds();

            foreach (Guid estateId in estateIdList)
            {
                String databaseName = $"EstateReportingReadModel{estateId}";

                // Build the connection string (to master)
                String connectionString = Setup.GetLocalConnectionString(databaseName);
                EstateReportingContext context = new EstateReportingContext(connectionString);
                await context.Database.EnsureDeletedAsync(CancellationToken.None);
            }
        }

        /// <summary>
        /// Stops the containers for scenario run.
        /// </summary>
        public override async Task StopContainersForScenarioRun()
        {
            await CleanUpSubscriptionServiceConfiguration().ConfigureAwait(false);

            await RemoveEstateReadModel().ConfigureAwait(false);

            if (this.Containers.Any())
            {
                foreach (IContainerService containerService in this.Containers)
                {
                    containerService.StopOnDispose = true;
                    containerService.RemoveOnDispose = true;
                    containerService.Dispose();
                }
            }

            if (this.TestNetworks.Any())
            {
                foreach (INetworkService networkService in this.TestNetworks)
                {
                    networkService.Stop();
                    networkService.Remove(true);
                }
            }
        }

        #endregion
    }
}