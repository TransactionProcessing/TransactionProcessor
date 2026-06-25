using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Moq;
using Shared.EntityFramework;
using Shouldly;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Repository;
using Xunit;

namespace TransactionProcessor.Tests.RepositoryTests
{
    public class TransactionProcessorReadModelRepositoryTests
    {
        private static (EstateManagementContext Context, DbConnection Connection) CreateContext()
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            DbContextOptionsBuilder<EstateManagementContext> builder = new DbContextOptionsBuilder<EstateManagementContext>()
                .UseSqlite(connection);

            EstateManagementContext context = new EstateManagementContext(builder.Options);
            context.Database.EnsureCreated();

            return (context, connection);
        }

        [Fact]
        public async Task GetEstate_DisposesResolvedScope_WhenReadCompletes()
        {
            Guid estateId = Guid.NewGuid();
            (EstateManagementContext context, DbConnection connection) = CreateContext();

            await using (context)
            await using (connection)
            {
                await context.Estates.AddAsync(new Estate
                {
                    EstateId = estateId,
                    CreatedDateTime = DateTime.UtcNow,
                    Name = "Test estate",
                    Reference = "EST-001"
                }, CancellationToken.None);
                await context.SaveChangesAsync(CancellationToken.None);

                TrackingDisposable trackingDisposable = new TrackingDisposable();
                ServiceProvider serviceProvider = new ServiceCollection()
                    .AddScoped(_ => trackingDisposable)
                    .AddTransient(_ => context)
                    .BuildServiceProvider();

                IServiceScope scope = serviceProvider.CreateScope();
                _ = scope.ServiceProvider.GetRequiredService<TrackingDisposable>();

                Mock<IDbContextResolver<EstateManagementContext>> resolver = new();
                resolver.Setup(r => r.Resolve(It.IsAny<String>(), It.IsAny<String>()))
                        .Returns(new ResolvedDbContext<EstateManagementContext>(scope));

                TransactionProcessorReadModelRepository repository = new TransactionProcessorReadModelRepository(resolver.Object);

                var result = await repository.GetEstate(estateId, CancellationToken.None);

                result.IsSuccess.ShouldBeTrue();
                trackingDisposable.Disposed.ShouldBeTrue();
            }
        }

        private sealed class TrackingDisposable : IDisposable
        {
            public Boolean Disposed { get; private set; }

            public void Dispose()
            {
                this.Disposed = true;
            }
        }
    }
}
