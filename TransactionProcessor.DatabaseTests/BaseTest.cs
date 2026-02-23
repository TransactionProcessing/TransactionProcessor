using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NLog;
using Shared.EntityFramework;
using Shared.IntegrationTesting;
using Shared.IntegrationTesting.TestContainers;
using Shared.Logger;
using Shouldly;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Repository;
using Xunit.Abstractions;
using Logger = Shared.Logger.Logger;

namespace TransactionProcessor.DatabaseTests
{
    public abstract class BaseTest : IAsyncLifetime
    {
        protected ITransactionProcessorReadModelRepository Repository;
        protected ITestOutputHelper TestOutputHelper;
        public virtual async Task InitializeAsync()
        {
            Logger.Initialise(new Shared.Logger.NullLogger());
            
            this.TestId = Guid.NewGuid();

            await this.StartSqlContainer();
            await this.GetRepository();
            EstateManagementContext context = this.GetContext();
            await context.Database.EnsureCreatedAsync(CancellationToken.None);
        }

        public EstateManagementContext GetContext()
        {
            return new EstateManagementContext(GetLocalConnectionString($"TransactionProcessorReadModel-{this.TestId}"));
        }

        public async Task GetRepository()
        {
            String dbConnString = GetLocalConnectionString($"TransactionProcessorReadModel-{this.TestId}");

            Mock<IDbContextResolver<EstateManagementContext>> resolver = new Mock<IDbContextResolver<EstateManagementContext>>();
            resolver.Setup(r => r.Resolve(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() =>
                {
                    Mock<IServiceScope> innerScope = new Mock<IServiceScope>();
                    EstateManagementContext context = new EstateManagementContext(dbConnString);

                    innerScope.Setup(s => s.ServiceProvider.GetService(typeof(EstateManagementContext)))
                        .Returns(context);

                    return new ResolvedDbContext<EstateManagementContext>(innerScope.Object);
                });

            this.Repository = new TransactionProcessorReadModelRepository(resolver.Object);
        }

        public virtual async Task DisposeAsync() {
            await this.DockerHelper.StopContainersForScenarioRun(DockerServices.None);
        }

        //protected abstract Task ClearStandingData();
        //protected abstract Task SetupStandingData();
        
        protected Guid TestId;
        
        public static INetwork DatabaseServerNetwork;
        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");

        public String GetLocalConnectionString(String databaseName)
        {
            Int32? databaseHostPort = DockerHelper.GetHostPort(ContainerType.SqlServer);
            
            return $"server=localhost,{databaseHostPort};database={databaseName};user id={SqlCredentials.usename};password={SqlCredentials.password};Encrypt=false";
        }

        private DockerHelper DockerHelper;

        internal async Task StartSqlContainer()
        {
            DockerHelper = new TestDockerHelper();

            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger("Specflow"), "Specflow");
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);
            DockerHelper.Logger = logger;
            DockerHelper.SqlCredentials = SqlCredentials;
            DockerHelper.SqlServerContainerName = $"sqlserver_{this.TestId}";
            DockerHelper.RequiredDockerServices = DockerServices.SqlServer;

            //DatabaseServerNetwork = await DockerHelper.SetupTestNetwork($"nw{this.TestId}", true);
            await DockerHelper.StartContainersForScenarioRun(this.TestId.ToString(), DockerServices.SqlServer);
        }

        //public void Dispose()
        //{
        //    EstateManagementContext context = new EstateManagementContext(this.GetLocalConnectionString($"EstateReportingReadModel{this.TestId.ToString()}"));

        //    Console.WriteLine($"About to delete database EstateReportingReadModel{this.TestId.ToString()}");
        //    Boolean result = context.Database.EnsureDeleted();
        //    Console.WriteLine($"Delete result is {result}");
        //    result.ShouldBeTrue();
            
        //}
    }



    public class TestDockerHelper : DockerHelper
    {
        public override async Task CreateSubscriptions()
        {
            // Nothing here
        }
    }

}
