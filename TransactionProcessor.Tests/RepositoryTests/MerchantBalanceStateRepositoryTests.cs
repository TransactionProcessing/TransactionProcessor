using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.EntityFramework;
using Shouldly;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.ProjectionEngine.Repository;
using Xunit;

namespace TransactionProcessor.Tests.RepositoryTests;

public class MerchantBalanceStateRepositoryTests
{
    [Fact]
    public async Task Load_ReturnsFailure_WhenQueryThrows()
    {
        Guid estateId = Guid.NewGuid();
        Guid merchantId = Guid.NewGuid();

        using SqliteConnection connection = new("DataSource=:memory:");
        await connection.OpenAsync();

        DbContextOptions<EstateManagementContext> setupOptions = new DbContextOptionsBuilder<EstateManagementContext>()
            .UseSqlite(connection)
            .Options;

        await using (EstateManagementContext setupContext = new(setupOptions))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        DbContextOptions<EstateManagementContext> failingOptions = new DbContextOptionsBuilder<EstateManagementContext>()
            .UseSqlite(connection)
            .AddInterceptors(new ThrowingDbCommandInterceptor())
            .Options;

        ServiceProvider serviceProvider = new ServiceCollection()
            .AddScoped(_ => new EstateManagementContext(failingOptions))
            .BuildServiceProvider();

        using IServiceScope scope = serviceProvider.CreateScope();

        Mock<IDbContextResolver<EstateManagementContext>> resolver = new();
        resolver.Setup(r => r.Resolve(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ResolvedDbContext<EstateManagementContext>(scope));

        MerchantBalanceStateRepository repository = new(resolver.Object);

        var result = await repository.Load(estateId, merchantId, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Message.ShouldContain("Error getting merchant balance state");
        result.Message.ShouldContain("Simulated transient SQL failure");
    }

    private sealed class ThrowingDbCommandInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
                                                                         CommandEventData eventData,
                                                                         InterceptionResult<DbDataReader> result)
        {
            throw new InvalidOperationException("Simulated transient SQL failure");
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
                                                                                          CommandEventData eventData,
                                                                                          InterceptionResult<DbDataReader> result,
                                                                                          CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated transient SQL failure");
        }
    }
}
