using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

[Collection(DatabaseTestCollection.Name)]
public class OperatorEventTests : BaseTest {
    public OperatorEventTests(DatabaseTestFixture fixture) : base(fixture) {
    }

    [Fact]
    public async Task AddOperator_OperatorIsAdded()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        EstateManagementContext context = this.GetContext();
        Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId, TestContext.Current.CancellationToken);
        @operator.ShouldNotBeNull();
        @operator.Name.ShouldBe(TestData.DomainEvents.OperatorCreatedEvent.Name);
        @operator.RequireCustomMerchantNumber.ShouldBe(TestData.DomainEvents.OperatorCreatedEvent.RequireCustomMerchantNumber);
        @operator.RequireCustomTerminalNumber.ShouldBe(TestData.DomainEvents.OperatorCreatedEvent.RequireCustomTerminalNumber);
    }

    [Fact]
    public async Task AddOperator_EventReplayHandled()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateOperatorName_OperatorIsUpdated()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateOperator(TestData.DomainEvents.OperatorNameUpdatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId, TestContext.Current.CancellationToken);
        @operator.ShouldNotBeNull();
        @operator.Name.ShouldBe(TestData.DomainEvents.OperatorNameUpdatedEvent.Name);
    }

    [Fact]
    public async Task UpdateOperatorRequireCustomMerchantNumber_OperatorIsUpdated()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateOperator(TestData.DomainEvents.OperatorRequireCustomMerchantNumberChangedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId, TestContext.Current.CancellationToken);
        @operator.ShouldNotBeNull();
        @operator.RequireCustomMerchantNumber.ShouldBe(TestData.DomainEvents.OperatorRequireCustomMerchantNumberChangedEvent.RequireCustomMerchantNumber);
    }

    [Fact]
    public async Task UpdateOperatorRequireCustomTerminalNumber_OperatorIsUpdated()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateOperator(TestData.DomainEvents.OperatorRequireCustomTerminalNumberChangedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId, TestContext.Current.CancellationToken);
        @operator.ShouldNotBeNull();
        @operator.RequireCustomTerminalNumber.ShouldBe(TestData.DomainEvents.OperatorRequireCustomTerminalNumberChangedEvent.RequireCustomTerminalNumber);
    }
}


