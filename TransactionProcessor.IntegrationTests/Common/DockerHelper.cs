namespace TransactionProcessor.IntegrationTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Client;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Executors;
    using Ductus.FluentDocker.Extensions;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using EstateManagement.Client;
    using global::Shared.Logger;
    using SecurityService.Client;

    public class DockerHelper
    {
        private readonly NlogLogger Logger;

        protected INetworkService TestNetwork;

        public Int32 SecurityServicePort;
        protected Int32 EstateManagementPort;
        protected Int32 TransactionProcessorPort;
        protected Int32 EventStorePort;

        public IContainerService SecurityServiceContainer;
        public IContainerService EstateManagementContainer;
        public IContainerService TransactionProcessorContainer;
        protected IContainerService EventStoreContainer;

        public IEstateClient EstateClient;
        public ITransactionProcessorClient TransactionProcessorClient;
        public ISecurityServiceClient SecurityServiceClient;

        protected String EventStoreConnectionString;

        public String SecurityServiceContainerName;
        protected String EstateManagementContainerName;
        protected String TransactionProcessorContainerName;
        protected String EventStoreContainerName;

        public DockerHelper(NlogLogger logger)
        {
            this.Logger = logger;
        }

        private void SetupTestNetwork()
        {
            // Build a network
            this.TestNetwork = new Ductus.FluentDocker.Builders.Builder().UseNetwork($"testnetwork{Guid.NewGuid()}").Build();
        }
        public Guid TestId;
        private void SetupEventStoreContainer(String traceFolder)
        {
            // Event Store Container
            this.EventStoreContainer = new Ductus.FluentDocker.Builders.Builder()
                                       .UseContainer()
                                       .UseImage("eventstore/eventstore:release-5.0.2")
                                       .ExposePort(2113)
                                       .ExposePort(1113)
                                       .WithName(this.EventStoreContainerName)
                                       .WithEnvironment("EVENTSTORE_RUN_PROJECTIONS=all", "EVENTSTORE_START_STANDARD_PROJECTIONS=true")
                                       .UseNetwork(this.TestNetwork)
                                       .Mount(traceFolder, "/var/log/eventstore", MountType.ReadWrite)
                                       .Build()
                                       .Start().WaitForPort("2113/tcp", 30000);
        }

        public async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = $"/home/ubuntu/estatemanagement/trace/{scenarioName}/";

            Logging.Enabled();

            Guid testGuid = Guid.NewGuid();
            this.TestId = testGuid;

            // Setup the container names
            this.SecurityServiceContainerName = $"securityservice{testGuid:N}";
            this.EstateManagementContainerName = $"estate{testGuid:N}";
            this.TransactionProcessorContainerName = $"txnprocessor{testGuid:N}";
            this.EventStoreContainerName = $"eventstore{testGuid:N}";
            
            this.EventStoreConnectionString =
                $"EventStoreSettings:ConnectionString=ConnectTo=tcp://admin:changeit@{this.EventStoreContainerName}:1113;VerboseLogging=true;";

            this.SetupTestNetwork();
            this.SetupSecurityServiceContainer(traceFolder);
            this.SetupEventStoreContainer(traceFolder);
            this.SetupEstateManagementContainer(traceFolder);
            this.SetupTransactionProcessorContainer(traceFolder);

            // Cache the ports
            this.EstateManagementPort = this.EstateManagementContainer.ToHostExposedEndpoint("5000/tcp").Port;
            this.TransactionProcessorPort = this.TransactionProcessorContainer.ToHostExposedEndpoint("5002/tcp").Port;
            this.EventStorePort = this.EventStoreContainer.ToHostExposedEndpoint("2113/tcp").Port;
            this.SecurityServicePort = this.SecurityServiceContainer.ToHostExposedEndpoint("5001/tcp").Port;

            // Setup the base address resolver
            Func<String, String> estateManagementBaseAddressResolver = api => $"http://127.0.0.1:{this.EstateManagementPort}";
            Func<String, String> transactionProcessorBaseAddressResolver = api => $"http://127.0.0.1:{this.TransactionProcessorPort}";
            Func<String, String> securityServiceBaseAddressResolver = api => $"http://127.0.0.1:{this.SecurityServicePort}";

            HttpClient httpClient = new HttpClient();
            this.EstateClient = new EstateClient(estateManagementBaseAddressResolver, httpClient);
            this.TransactionProcessorClient = new TransactionProcessorClient(transactionProcessorBaseAddressResolver, httpClient);
            this.SecurityServiceClient = new SecurityServiceClient(securityServiceBaseAddressResolver, httpClient);
            // TODO: Use this to talk to txn processor until we have a client
            //this.HttpClient = new HttpClient();
            //this.HttpClient.BaseAddress = new Uri(transactionProcessorBaseAddressResolver(String.Empty));
        }

        public async Task StopContainersForScenarioRun()
        {
            try
            {
                if (this.SecurityServiceContainer != null)
                {
                    this.SecurityServiceContainer.StopOnDispose = true;
                    this.SecurityServiceContainer.RemoveOnDispose = true;
                    this.SecurityServiceContainer.Dispose();
                }

                if (this.TransactionProcessorContainer != null)
                {
                    this.TransactionProcessorContainer.StopOnDispose = true;
                    this.TransactionProcessorContainer.RemoveOnDispose = true;
                    this.TransactionProcessorContainer.Dispose();
                }

                if (this.EstateManagementContainer != null)
                {
                    this.EstateManagementContainer.StopOnDispose = true;
                    this.EstateManagementContainer.RemoveOnDispose = true;
                    this.EstateManagementContainer.Dispose();
                }

                if (this.EventStoreContainer != null)
                {
                    this.EventStoreContainer.StopOnDispose = true;
                    this.EventStoreContainer.RemoveOnDispose = true;
                    this.EventStoreContainer.Dispose();
                }

                if (this.TestNetwork != null)
                {
                    this.TestNetwork.Stop();
                    this.TestNetwork.Remove(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetupSecurityServiceContainer(String traceFolder)
        {
            this.Logger.LogInformation("About to Start Security Container");

            this.SecurityServiceContainer = new Builder().UseContainer().WithName(this.SecurityServiceContainerName)
                                                         .WithEnvironment($"ServiceOptions:PublicOrigin=http://{this.SecurityServiceContainerName}:5001",
                                                                          $"ServiceOptions:IssuerUrl=http://{this.SecurityServiceContainerName}:5001",
                                                                          "ASPNETCORE_ENVIRONMENT=IntegrationTest",
                                                                          "urls=http://*:5001")
                                                         .WithCredential("https://www.docker.com", "stuartferguson", "Sc0tland")
                                                         .UseImage("stuartferguson/securityservice").ExposePort(5001).UseNetwork(new List<INetworkService>
                                                                                                                                 {
                                                                                                                                     this.TestNetwork
                                                                                                                                 }.ToArray())
                                                         .Mount(traceFolder, "/home/txnproc/trace", MountType.ReadWrite).Build().Start().WaitForPort("5001/tcp", 30000);
            Thread.Sleep(20000);

            this.Logger.LogInformation("Security Service Container Started");

        }

        private void SetupEstateManagementContainer(String traceFolder)
        {
            // Management API Container
            this.EstateManagementContainer = new Builder()
                                                .UseContainer()
                                                .WithName(this.EstateManagementContainerName)
                                                .WithEnvironment(this.EventStoreConnectionString,
                                                                 $"AppSettings:SecurityService=http://{this.SecurityServiceContainerName}:5001",
                                                                 $"SecurityConfiguration:Authority=http://{this.SecurityServiceContainerName}:5001",
                                                                 "urls=http://*:5000") //,
                                                //"AppSettings:MigrateDatabase=true",
                                                //"EventStoreSettings:START_PROJECTIONS=true",
                                                //"EventStoreSettings:ContinuousProjectionsFolder=/app/projections/continuous")
                                                .WithCredential("https://www.docker.com", "stuartferguson", "Sc0tland")
                                                .UseImage("stuartferguson/estatemanagement")
                                                .ExposePort(5000)
                                                .UseNetwork(new List<INetworkService> { this.TestNetwork, Setup.DatabaseServerNetwork }.ToArray())
                                                .Mount(traceFolder, "/home", MountType.ReadWrite)
                                                .Build()
                                                .Start().WaitForPort("5000/tcp", 30000);
        }

        private void SetupTransactionProcessorContainer(String traceFolder)
        {
            // Management API Container
            this.TransactionProcessorContainer = new Builder()
                                                .UseContainer()
                                                .WithName(this.TransactionProcessorContainerName)
                                                .WithEnvironment(this.EventStoreConnectionString,
                                                                 $"AppSettings:SecurityService=http://{this.SecurityServiceContainerName}:5001",
                                                                 $"SecurityConfiguration:Authority=http://{this.SecurityServiceContainerName}:5001") //,
                                                //"AppSettings:MigrateDatabase=true",
                                                //"EventStoreSettings:START_PROJECTIONS=true",
                                                //"EventStoreSettings:ContinuousProjectionsFolder=/app/projections/continuous")
                                                .UseImage("transactionprocessor")
                                                .ExposePort(5002)
                                                .UseNetwork(new List<INetworkService> { this.TestNetwork, Setup.DatabaseServerNetwork }.ToArray())
                                                .Mount(traceFolder, "/home", MountType.ReadWrite)
                                                .Build()
                                                .Start().WaitForPort("5002/tcp", 30000);
        }
    }
}
