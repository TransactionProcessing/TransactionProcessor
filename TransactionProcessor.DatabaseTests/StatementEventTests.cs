using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class StatementEventTests : BaseTest {
    [Fact]
    public async Task CreateStatement_StatementIsAdded()
    {
        Result result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        StatementHeader? statement = await context.StatementHeaders.SingleOrDefaultAsync(c => c.StatementId == TestData.DomainEvents.StatementCreatedEvent.MerchantStatementId);
        statement.ShouldNotBeNull();
    }
    [Fact]
    public async Task CreateStatement_EventReplayHandled()
    {
        Result result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
}