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
        statement.StatementCreatedDate.ShouldBe(TestData.DomainEvents.StatementCreatedEvent.StatementDate.Date);
        statement.StatementCreatedDateTime.ShouldBe(TestData.DomainEvents.StatementCreatedEvent.StatementDate);
    }
    [Fact]
    public async Task CreateStatement_EventReplayHandled()
    {
        Result result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MarkStatementAsGenerated_StatementIsUpdated()
    {
        Result result = await this.Repository.CreateStatement(TestData.DomainEvents.StatementCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.MarkStatementAsGenerated(TestData.DomainEvents.StatementGeneratedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        StatementHeader? statement = await context.StatementHeaders.SingleOrDefaultAsync(c => c.StatementId == TestData.DomainEvents.StatementCreatedEvent.MerchantStatementId);
        statement.ShouldNotBeNull();
        statement.StatementGeneratedDate.ShouldBe(TestData.DomainEvents.StatementGeneratedEvent.DateGenerated);
    }
}
