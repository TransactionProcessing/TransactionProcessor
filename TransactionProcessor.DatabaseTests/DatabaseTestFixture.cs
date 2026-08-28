using DotNet.Testcontainers.Builders;
using NLog;
using Shared.IntegrationTesting;
using Shared.IntegrationTesting.TestContainers;
using Shared.Logger;
using Logger = Shared.Logger.Logger;

namespace TransactionProcessor.DatabaseTests
{
    public sealed class DatabaseTestFixture : IAsyncLifetime
    {
        private readonly DockerHelper dockerHelper;

        public DatabaseTestFixture()
        {
            Logger.Initialise(new Shared.Logger.NullLogger());

            this.dockerHelper = new TestDockerHelper
            {
                Logger = this.CreateLogger(),
                SqlCredentials = BaseTest.SqlCredentials,
                RequiredDockerServices = DockerServices.SqlServer,
                SqlServerContainerName = "sqlserver_database_tests"
            };
        }

        public async ValueTask InitializeAsync()
        {
            await this.dockerHelper.StartContainersForScenarioRun("database-tests", DockerServices.SqlServer);
        }

        public async ValueTask DisposeAsync()
        {
            await this.dockerHelper.StopContainersForScenarioRun(DockerServices.SqlServer);
        }

        public string GetLocalConnectionString(string databaseName)
        {
            int? databaseHostPort = this.dockerHelper.GetHostPort(ContainerType.SqlServer);

            return $"server=localhost,{databaseHostPort};database={databaseName};user id={BaseTest.SqlCredentials.usename};password={BaseTest.SqlCredentials.password};Encrypt=false";
        }

        private NlogLogger CreateLogger()
        {
            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger("Specflow"), "Specflow");
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);
            return logger;
        }
    }

    public class TestDockerHelper : DockerHelper
    {
        public override Task CreateSubscriptions()
        {
            return Task.CompletedTask;
        }
    }

}
