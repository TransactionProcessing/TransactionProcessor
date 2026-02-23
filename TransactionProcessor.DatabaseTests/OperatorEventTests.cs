using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class OperatorEventTests : BaseTest {
    [Fact]
    public async Task AddOperator_OperatorIsAdded()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId);
        @operator.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddOperator_EventReplayHandled()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
}