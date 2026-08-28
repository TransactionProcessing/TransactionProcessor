using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

public class FloatEventTests : BaseTest {
    public FloatEventTests(DatabaseTestFixture fixture) : base(fixture) {
    }

    [Fact]
    public async Task CreateFloat_FloatIsAdded()
    {
        Result result = await this.Repository.CreateFloat(TestData.DomainEvents.FloatCreatedForContractProductEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Float? floatRecord = await context.Floats.SingleOrDefaultAsync(f => f.FloatId == TestData.DomainEvents.FloatCreatedForContractProductEvent.FloatId, TestContext.Current.CancellationToken);
        floatRecord.ShouldNotBeNull();
        floatRecord.EstateId.ShouldBe(TestData.DomainEvents.FloatCreatedForContractProductEvent.EstateId);
        floatRecord.CreatedDate.ShouldBe(TestData.FloatCreatedDateTime.Date);
        floatRecord.CreatedDateTime.ShouldBe(TestData.FloatCreatedDateTime);
    }

    [Fact]
    public async Task CreateFloat_EventReplayHandled()
    {
        FloatDomainEvents.FloatCreatedForContractProductEvent @event = TestData.DomainEvents.FloatCreatedForContractProductEvent;

        Result result = await this.Repository.CreateFloat(@event, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.CreateFloat(@event, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Int32 floatCount = await context.Floats.CountAsync(f => f.FloatId == @event.FloatId, TestContext.Current.CancellationToken);
        floatCount.ShouldBe(1);
    }

    [Fact]
    public async Task CreateFloatActivity_CreditPurchaseIsAdded()
    {
        FloatDomainEvents.FloatCreditPurchasedEvent @event = new(
            TestData.FloatAggregateId,
            TestData.EstateId,
            TestData.CreditPurchasedDateTime,
            TestData.FloatCreditAmount,
            TestData.FloatCreditCostPrice);

        Result result = await this.Repository.CreateFloatActivity(@event, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        FloatActivity? floatActivity = await context.FloatActivity.SingleOrDefaultAsync(f => f.EventId == @event.EventId, TestContext.Current.CancellationToken);
        floatActivity.ShouldNotBeNull();
        floatActivity.FloatId.ShouldBe(@event.FloatId);
        floatActivity.ActivityDate.ShouldBe(TestData.CreditPurchasedDateTime.Date);
        floatActivity.ActivityDateTime.ShouldBe(TestData.CreditPurchasedDateTime);
        floatActivity.Amount.ShouldBe(TestData.FloatCreditAmount);
        floatActivity.CostPrice.ShouldBe(TestData.FloatCreditCostPrice);
        floatActivity.CreditOrDebit.ShouldBe("C");
    }

    [Fact]
    public async Task CreateFloatActivity_CreditPurchaseReplayHandled()
    {
        FloatDomainEvents.FloatCreditPurchasedEvent @event = new(
            TestData.FloatAggregateId,
            TestData.EstateId,
            TestData.CreditPurchasedDateTime,
            TestData.FloatCreditAmount,
            TestData.FloatCreditCostPrice);

        Result result = await this.Repository.CreateFloatActivity(@event, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.CreateFloatActivity(@event, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Int32 floatActivityCount = await context.FloatActivity.CountAsync(f => f.EventId == @event.EventId, TestContext.Current.CancellationToken);
        floatActivityCount.ShouldBe(1);
    }

    [Fact]
    public async Task CreateFloatActivity_FloatDecreaseIsAdded()
    {
        await this.Repository.StartTransaction(TestData.DomainEvents.TransactionHasStartedEvent, TestContext.Current.CancellationToken);

        FloatDomainEvents.FloatDecreasedByTransactionEvent @event = new(
            TestData.FloatAggregateId,
            TestData.EstateId,
            TestData.TransactionId,
            TestData.FloatCreditAmount);

        Result result = await this.Repository.CreateFloatActivity(@event, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        FloatActivity? floatActivity = await context.FloatActivity.SingleOrDefaultAsync(f => f.EventId == @event.EventId, TestContext.Current.CancellationToken);
        floatActivity.ShouldNotBeNull();
        floatActivity.FloatId.ShouldBe(@event.FloatId);
        floatActivity.ActivityDate.ShouldBe(TestData.TransactionDateTime1.Date);
        floatActivity.ActivityDateTime.ShouldBe(TestData.TransactionDateTime1);
        floatActivity.Amount.ShouldBe(TestData.FloatCreditAmount);
        floatActivity.CostPrice.ShouldBe(0);
        floatActivity.CreditOrDebit.ShouldBe("D");
    }

    [Fact]
    public async Task CreateFloatActivity_FloatDecreaseReplayHandled()
    {
        await this.Repository.StartTransaction(TestData.DomainEvents.TransactionHasStartedEvent, TestContext.Current.CancellationToken);

        FloatDomainEvents.FloatDecreasedByTransactionEvent @event = new(
            TestData.FloatAggregateId,
            TestData.EstateId,
            TestData.TransactionId,
            TestData.FloatCreditAmount);

        Result result = await this.Repository.CreateFloatActivity(@event, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.CreateFloatActivity(@event, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Int32 floatActivityCount = await context.FloatActivity.CountAsync(f => f.EventId == @event.EventId, TestContext.Current.CancellationToken);
        floatActivityCount.ShouldBe(1);
    }

}


