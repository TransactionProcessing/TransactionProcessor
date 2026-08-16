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
        @operator.Name.ShouldBe(TestData.DomainEvents.OperatorCreatedEvent.Name);
        @operator.RequireCustomMerchantNumber.ShouldBe(TestData.DomainEvents.OperatorCreatedEvent.RequireCustomMerchantNumber);
        @operator.RequireCustomTerminalNumber.ShouldBe(TestData.DomainEvents.OperatorCreatedEvent.RequireCustomTerminalNumber);
    }

    [Fact]
    public async Task AddOperator_EventReplayHandled()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateOperatorName_OperatorIsUpdated()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateOperator(TestData.DomainEvents.OperatorNameUpdatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId);
        @operator.ShouldNotBeNull();
        @operator.Name.ShouldBe(TestData.DomainEvents.OperatorNameUpdatedEvent.Name);
    }

    [Fact]
    public async Task UpdateOperatorRequireCustomMerchantNumber_OperatorIsUpdated()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateOperator(TestData.DomainEvents.OperatorRequireCustomMerchantNumberChangedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId);
        @operator.ShouldNotBeNull();
        @operator.RequireCustomMerchantNumber.ShouldBe(TestData.DomainEvents.OperatorRequireCustomMerchantNumberChangedEvent.RequireCustomMerchantNumber);
    }

    [Fact]
    public async Task UpdateOperatorRequireCustomTerminalNumber_OperatorIsUpdated()
    {
        Result result = await this.Repository.AddOperator(TestData.DomainEvents.OperatorCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.UpdateOperator(TestData.DomainEvents.OperatorRequireCustomTerminalNumberChangedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Operator? @operator = await context.Operators.SingleOrDefaultAsync(c => c.OperatorId == TestData.DomainEvents.OperatorCreatedEvent.OperatorId);
        @operator.ShouldNotBeNull();
        @operator.RequireCustomTerminalNumber.ShouldBe(TestData.DomainEvents.OperatorRequireCustomTerminalNumberChangedEvent.RequireCustomTerminalNumber);
    }
}
