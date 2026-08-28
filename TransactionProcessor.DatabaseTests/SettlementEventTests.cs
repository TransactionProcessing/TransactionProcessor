using Microsoft.EntityFrameworkCore;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Testing;

namespace TransactionProcessor.DatabaseTests;

[Collection(DatabaseTestCollection.Name)]
public class SettlementEventTests : BaseTest
{
    public SettlementEventTests(DatabaseTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateSettlement_SettlementIsAdded()
    {
        Result result = await this.Repository.CreateSettlement(TestData.DomainEvents.SettlementCreatedForDateEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Settlement? settlement = await context.Settlements.SingleOrDefaultAsync(c => c.SettlementId == TestData.DomainEvents.SettlementCreatedForDateEvent.SettlementId, TestContext.Current.CancellationToken);
        settlement.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateSettlement_EventReplayHandled()
    {
        Result result = await this.Repository.CreateSettlement(TestData.DomainEvents.SettlementCreatedForDateEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.CreateSettlement(TestData.DomainEvents.SettlementCreatedForDateEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MarkSettlementAsProcessingStarted_SettlementIsUpdated()
    {
        Result result = await this.Repository.CreateSettlement(TestData.DomainEvents.SettlementCreatedForDateEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.MarkSettlementAsProcessingStarted(TestData.DomainEvents.SettlementProcessingStartedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Settlement? settlement = await context.Settlements.SingleOrDefaultAsync(c => c.SettlementId == TestData.DomainEvents.SettlementProcessingStartedEvent.SettlementId, TestContext.Current.CancellationToken);
        settlement.ShouldNotBeNull();
        settlement.ProcessingStarted.ShouldBeTrue();
        settlement.ProcessingStartedDateTIme.ShouldBe(TestData.DomainEvents.SettlementProcessingStartedEvent.ProcessingStartedDateTime);
    }

    [Fact]
    public async Task MarkSettlementAsCompleted_SettlementIsUpdated()
    {
        Result result = await this.Repository.CreateSettlement(TestData.DomainEvents.SettlementCreatedForDateEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.MarkSettlementAsCompleted(TestData.DomainEvents.SettlementCompletedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        Settlement? settlement = await context.Settlements.SingleOrDefaultAsync(c => c.SettlementId == TestData.DomainEvents.SettlementCompletedEvent.SettlementId, TestContext.Current.CancellationToken);
        settlement.ShouldNotBeNull();
        settlement.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task AddPendingMerchantFeeToSettlement_MerchantFeeIsAdded()
    {
        Result result = await this.Repository.AddPendingMerchantFeeToSettlement(TestData.DomainEvents.MerchantFeeAddedPendingSettlementEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantSettlementFee? merchantSettlementFee = await context.MerchantSettlementFees.SingleOrDefaultAsync(f =>
            f.SettlementId == TestData.DomainEvents.MerchantFeeAddedPendingSettlementEvent.SettlementId &&
            f.TransactionId == TestData.DomainEvents.MerchantFeeAddedPendingSettlementEvent.TransactionId &&
            f.ContractProductTransactionFeeId == TestData.DomainEvents.MerchantFeeAddedPendingSettlementEvent.FeeId,
            TestContext.Current.CancellationToken);

        merchantSettlementFee.ShouldNotBeNull();
        merchantSettlementFee.IsSettled.ShouldBeFalse();
    }

    [Fact]
    public async Task AddPendingMerchantFeeToSettlement_EventReplayHandled()
    {
        Result result = await this.Repository.AddPendingMerchantFeeToSettlement(TestData.DomainEvents.MerchantFeeAddedPendingSettlementEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddPendingMerchantFeeToSettlement(TestData.DomainEvents.MerchantFeeAddedPendingSettlementEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AddSettledMerchantFeeToSettlement_MerchantFeeIsAdded()
    {
        Result result = await this.Repository.AddSettledMerchantFeeToSettlement(TestData.SettledMerchantFeeAddedToTransactionEvent(TestData.SettlementDate), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantSettlementFee? merchantSettlementFee = await context.MerchantSettlementFees.SingleOrDefaultAsync(f =>
            f.SettlementId == TestData.SettlementAggregateId &&
            f.TransactionId == TestData.SettlementAggregateId &&
            f.ContractProductTransactionFeeId == TestData.TransactionFeeId,
            TestContext.Current.CancellationToken);

        merchantSettlementFee.ShouldNotBeNull();
        merchantSettlementFee.IsSettled.ShouldBeTrue();
    }

    [Fact]
    public async Task AddSettledMerchantFeeToSettlement_EventReplayHandled()
    {
        Result result = await this.Repository.AddSettledMerchantFeeToSettlement(TestData.SettledMerchantFeeAddedToTransactionEvent(TestData.SettlementDate), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.AddSettledMerchantFeeToSettlement(TestData.SettledMerchantFeeAddedToTransactionEvent(TestData.SettlementDate), TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MarkMerchantFeeAsSettled_MerchantFeeIsMarkedSettled()
    {
        SettlementDomainEvents.MerchantFeeAddedPendingSettlementEvent pendingEvent = new(
            TestData.SettlementId,
            TestData.EstateId,
            TestData.MerchantId,
            TestData.TransactionId,
            TestData.CalculatedFeeValue,
            TestData.FeeCalculationType,
            TestData.SettledFeeId1,
            TestData.FeeValue,
            TestData.TransactionFeeCalculateDateTime);

        Result result = await this.Repository.AddPendingMerchantFeeToSettlement(pendingEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        result = await this.Repository.MarkMerchantFeeAsSettled(TestData.DomainEvents.MerchantFeeSettledEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

        EstateManagementContext context = this.GetContext();
        MerchantSettlementFee? merchantSettlementFee = await context.MerchantSettlementFees.SingleOrDefaultAsync(f =>
            f.SettlementId == TestData.DomainEvents.MerchantFeeSettledEvent.SettlementId &&
            f.TransactionId == TestData.DomainEvents.MerchantFeeSettledEvent.TransactionId &&
            f.ContractProductTransactionFeeId == TestData.DomainEvents.MerchantFeeSettledEvent.FeeId,
            TestContext.Current.CancellationToken);

        merchantSettlementFee.ShouldNotBeNull();
        merchantSettlementFee.IsSettled.ShouldBeTrue();
    }
}

