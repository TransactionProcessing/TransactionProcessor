using Microsoft.EntityFrameworkCore;
using Shouldly;
using TransactionProcessing.UnionResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class StatementEventTests : BaseTest {
    public StatementEventTests(DatabaseTestFixture fixture) : base(fixture) {
    }

    [Fact]
    public async Task CreateStatement_StatementIsAdded()
    {
        Result result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        StatementHeader? statement = await context.StatementHeaders.SingleOrDefaultAsync(c => c.StatementId == TestData.DomainEvents.StatementCreatedEvent.MerchantStatementId, TestContext.Current.CancellationToken);
        statement.ShouldNotBeNull();
        statement.StatementCreatedDate.ShouldBe(DateOnly.FromDateTime(TestData.DomainEvents.StatementCreatedEvent.StatementDate.Date));
        statement.StatementCreatedDateTime.ShouldBe(TestData.DomainEvents.StatementCreatedEvent.StatementDate);
    }
    [Fact]
    public async Task CreateStatement_EventReplayHandled()
    {
        Result result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MarkStatementAsGenerated_StatementIsUpdated()
    {
        Result result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.MarkStatementAsGenerated(TestData.DomainEvents.StatementGeneratedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        StatementHeader? statement = await context.StatementHeaders.SingleOrDefaultAsync(c => c.StatementId == TestData.DomainEvents.StatementCreatedEvent.MerchantStatementId, TestContext.Current.CancellationToken);
        statement.ShouldNotBeNull();
        statement.StatementGeneratedDate.ShouldBe(DateOnly.FromDateTime(TestData.DomainEvents.StatementGeneratedEvent.DateGenerated.DateTime));
    }
}

