using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.EntityFramework;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Repository;

namespace TransactionProcessor.DatabaseTests
{
    public abstract class BaseTest : IAsyncLifetime
    {
        private readonly DatabaseTestFixture fixture;
        protected ITransactionProcessorReadModelRepository Repository = null!;
        protected Guid TestId;

        protected BaseTest(DatabaseTestFixture fixture)
        {
            this.fixture = fixture;
            this.TestId = Guid.NewGuid();
        }

        public virtual async ValueTask InitializeAsync()
        {
            await this.GetRepository();
            EstateManagementContext context = this.GetContext();
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        }

        public EstateManagementContext GetContext()
        {
            return new EstateManagementContext(this.GetLocalConnectionString($"TransactionProcessorReadModel-{this.TestId}"));
        }

        public async Task GetRepository()
        {
            string dbConnString = this.GetLocalConnectionString($"TransactionProcessorReadModel-{this.TestId}");

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

        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");

        public String GetLocalConnectionString(String databaseName)
        {
            return this.fixture.GetLocalConnectionString(databaseName);
        }
    }
}
