using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NLog;
using Shared.EntityFramework;
using Shared.IntegrationTesting;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Shared.IntegrationTesting.Ductus;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Repository;
using Xunit.Abstractions;
using Logger = Shared.Logger.Logger;
using NullLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger;

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

        public virtual async Task DisposeAsync()
        {
        }

        //protected abstract Task ClearStandingData();
        //protected abstract Task SetupStandingData();
        
        protected Guid TestId;
        
        public static IContainerService DatabaseServerContainer;
        public static INetworkService DatabaseServerNetwork;
        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");

        public static String GetLocalConnectionString(String databaseName)
        {
            Int32 databaseHostPort = DatabaseServerContainer.ToHostExposedEndpoint("1433/tcp").Port;

            return $"server=localhost,{databaseHostPort};database={databaseName};user id={SqlCredentials.usename};password={SqlCredentials.password};Encrypt=false";
        }

        internal async Task StartSqlContainer()
        {
            DockerHelper dockerHelper = new TestDockerHelper();

            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger("Specflow"), "Specflow");
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);
            dockerHelper.Logger = logger;
            dockerHelper.SqlCredentials = SqlCredentials;
            dockerHelper.SqlServerContainerName = "sharedsqlserver_repotests";
            dockerHelper.RequiredDockerServices = DockerServices.SqlServer;

            DatabaseServerNetwork = dockerHelper.SetupTestNetwork("sharednetwork", true);
            await Retry.For(async () => {
                DatabaseServerContainer = await dockerHelper.SetupSqlServerContainer(DatabaseServerNetwork);
            });
        }

        public void Dispose()
        {
            EstateManagementContext context = new EstateManagementContext(BaseTest.GetLocalConnectionString($"EstateReportingReadModel{this.TestId.ToString()}"));

            Console.WriteLine($"About to delete database EstateReportingReadModel{this.TestId.ToString()}");
            Boolean result = context.Database.EnsureDeleted();
            Console.WriteLine($"Delete result is {result}");
            result.ShouldBeTrue();
        }
    }



    public class TestDockerHelper : DockerHelper
    {
        public override async Task CreateSubscriptions()
        {
            // Nothing here
        }
    }

}
