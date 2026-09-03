using Microsoft.Extensions.DependencyInjection;
using Imposter.Abstractions;
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

            IDbContextResolverImposter<EstateManagementContext> resolver = new();
            resolver.Resolve(Arg<string>.Any(), Arg<string>.Any())
                .Returns((_, _) =>
                {
                    EstateManagementContext context = new EstateManagementContext(dbConnString);
                    ServiceProvider serviceProvider = new ServiceCollection()
                        .AddSingleton(context)
                        .BuildServiceProvider();
                    return new ResolvedDbContext<EstateManagementContext>(serviceProvider.CreateScope());
                });

            this.Repository = new TransactionProcessorReadModelRepository(resolver.Instance());
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
