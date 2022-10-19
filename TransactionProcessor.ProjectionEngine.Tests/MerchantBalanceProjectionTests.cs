namespace TransactionProcessor.ProjectionEngine.Tests;

using EstateManagement.Merchant.DomainEvents;
using Projections;
using Shouldly;
using State;
using Transaction.DomainEvents;

public class MerchantBalanceProjectionTests
{
    [Fact]
    public async Task MerchantBalanceProjection_Handle_MerchantCreatedEvent_EventIsHandled() {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantCreatedEvent @event = TestData.MerchantCreatedEvent;
            
        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.EstateId.ShouldBe(TestData.EstateId);
        newState.MerchantId.ShouldBe(TestData.MerchantId);
        newState.MerchantName.ShouldBe(TestData.MerchantName);
        newState.AvailableBalance.ShouldBe(0);
        newState.Balance.ShouldBe(0);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_ManualDepositMadeEvent_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with {
                               EstateId = TestData.EstateId,
                               MerchantId = TestData.MerchantId,
                               MerchantName = TestData.MerchantName
                           };

        ManualDepositMadeEvent @event = TestData.ManualDepositMadeEvent;

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(TestData.ManualDepositMadeEvent.Amount);
        newState.Balance.ShouldBe(TestData.ManualDepositMadeEvent.Amount);
        newState.DepositCount.ShouldBe(1);
        newState.TotalDeposited.ShouldBe(TestData.ManualDepositMadeEvent.Amount);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_AutomaticDepositMadeEvent_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName
                };

        AutomaticDepositMadeEvent @event = TestData.AutomaticDepositMadeEvent;

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount);
        newState.Balance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount);
        newState.DepositCount.ShouldBe(1);
        newState.TotalDeposited.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_TransactionHasStartedEvent_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 100.00m,
                    Balance = 100.00m
                };

        TransactionHasStartedEvent @event = TestData.GetTransactionHasStartedEvent(TestData.TransactionAmount);

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance - @event.TransactionAmount.GetValueOrDefault(0));
        newState.Balance.ShouldBe(state.Balance);
        newState.StartedTransactionCount.ShouldBe(1);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_TransactionHasCompletedEvent_IsAuthorised_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 75.00m,
                    Balance = 100.00m
                };
        TransactionHasBeenCompletedEvent @event = TestData.GetTransactionHasBeenCompletedEvent(true, TestData.TransactionAmount);

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance);
        newState.Balance.ShouldBe(state.Balance - @event.TransactionAmount.GetValueOrDefault(0));
        newState.AuthorisedSales.ShouldBe(@event.TransactionAmount.GetValueOrDefault(0));
        newState.SaleCount.ShouldBe(1);
        newState.CompletedTransactionCount.ShouldBe(1);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_TransactionHasCompletedEvent_IsNotAuthorised_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 75.00m,
                    Balance = 100.00m
                };
        TransactionHasBeenCompletedEvent @event = TestData.GetTransactionHasBeenCompletedEvent(false, TestData.TransactionAmount);

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance + @event.TransactionAmount.GetValueOrDefault(0));
        newState.Balance.ShouldBe(state.Balance);
        newState.DeclinedSales.ShouldBe(@event.TransactionAmount.GetValueOrDefault(0));
        newState.SaleCount.ShouldBe(1);
        newState.CompletedTransactionCount.ShouldBe(1);
    }
    
    [Fact]
    public async Task MerchantBalanceProjection_Handle_MerchantFeeAddedToTransactionEvent_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 75.00m,
                    Balance = 75.00m
                };
        MerchantFeeAddedToTransactionEvent @event = TestData.GetMerchantFeeAddedToTransactionEvent(0.25m);

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance + @event.CalculatedValue);
        newState.Balance.ShouldBe(state.Balance + @event.CalculatedValue);
        newState.ValueOfFees.ShouldBe(@event.CalculatedValue);
        newState.FeeCount.ShouldBe(1);
    }
}