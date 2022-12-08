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
        newState.DepositCount.ShouldBe(0);
        newState.StartedTransactionCount.ShouldBe(0);
        newState.CompletedTransactionCount.ShouldBe(0);
        newState.SaleCount.ShouldBe(0);
        newState.LastDeposit.ShouldBe(DateTime.MinValue);
        newState.LastFee.ShouldBe(DateTime.MinValue);
        newState.LastSale.ShouldBe(DateTime.MinValue);
        newState.AuthorisedSales.ShouldBe(0);
        newState.DeclinedSales.ShouldBe(0);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_ManualDepositMadeEvent_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    DepositCount = 0,
                    TotalDeposited = 0,
                    LastDeposit = DateTime.MinValue
                };

        ManualDepositMadeEvent @event = TestData.ManualDepositMadeEvent;

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(TestData.ManualDepositMadeEvent.Amount);
        newState.Balance.ShouldBe(TestData.ManualDepositMadeEvent.Amount);
        newState.DepositCount.ShouldBe(1);
        newState.TotalDeposited.ShouldBe(TestData.ManualDepositMadeEvent.Amount);
        newState.LastDeposit.ShouldBe(TestData.ManualDepositMadeEvent.DepositDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_WithdrawalMadeEvent_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 100.00m,
                    Balance = 100.00m,
                    WithdrawalCount = 0,
                    TotalWithdrawn = 0,
                    LastWithdrawal = DateTime.MinValue
                };

        WithdrawalMadeEvent @event = TestData.WithdrawalMadeEvent;

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance - TestData.WithdrawalMadeEvent.Amount);
        newState.Balance.ShouldBe(state.Balance - TestData.WithdrawalMadeEvent.Amount);
        newState.WithdrawalCount.ShouldBe(1);
        newState.TotalWithdrawn.ShouldBe(TestData.WithdrawalMadeEvent.Amount);
        newState.LastWithdrawal.ShouldBe(TestData.WithdrawalMadeEvent.WithdrawalDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_ManualDepositMadeEvent_SecondDepositAfterFirst_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
        {
            EstateId = TestData.EstateId,
            MerchantId = TestData.MerchantId,
            MerchantName = TestData.MerchantName,
            Balance = TestData.ManualDepositMadeEvent.Amount,
            AvailableBalance = TestData.ManualDepositMadeEvent.Amount,
            LastDeposit = TestData.ManualDepositMadeEvent.DepositDateTime.AddDays(-1),
            DepositCount = 1,
            TotalDeposited = TestData.ManualDepositMadeEvent.Amount,
        };

        ManualDepositMadeEvent @event = TestData.ManualDepositMadeEvent;

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(TestData.ManualDepositMadeEvent.Amount + @event.Amount);
        newState.Balance.ShouldBe(TestData.ManualDepositMadeEvent.Amount + @event.Amount);
        newState.DepositCount.ShouldBe(2);
        newState.TotalDeposited.ShouldBe(TestData.ManualDepositMadeEvent.Amount + @event.Amount);
        newState.LastDeposit.ShouldBe(TestData.ManualDepositMadeEvent.DepositDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_ManualDepositMadeEvent_SecondDepositBeforeFirst_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
        {
            EstateId = TestData.EstateId,
            MerchantId = TestData.MerchantId,
            MerchantName = TestData.MerchantName,
            Balance = TestData.ManualDepositMadeEvent.Amount,
            AvailableBalance = TestData.ManualDepositMadeEvent.Amount,
            LastDeposit = TestData.ManualDepositMadeEvent.DepositDateTime.AddDays(1),
            DepositCount = 1,
            TotalDeposited = TestData.ManualDepositMadeEvent.Amount,
        };

        ManualDepositMadeEvent @event = TestData.ManualDepositMadeEvent;

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(TestData.ManualDepositMadeEvent.Amount + @event.Amount);
        newState.Balance.ShouldBe(TestData.ManualDepositMadeEvent.Amount + @event.Amount);
        newState.DepositCount.ShouldBe(2);
        newState.TotalDeposited.ShouldBe(TestData.ManualDepositMadeEvent.Amount + @event.Amount);
        newState.LastDeposit.ShouldBe(state.LastDeposit);
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
                    MerchantName = TestData.MerchantName,
                    DepositCount = 0,
                    TotalDeposited = 0,
        };

        AutomaticDepositMadeEvent @event = TestData.AutomaticDepositMadeEvent;

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount);
        newState.Balance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount);
        newState.DepositCount.ShouldBe(1);
        newState.TotalDeposited.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount);
        newState.LastDeposit.ShouldBe(TestData.AutomaticDepositMadeEvent.DepositDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_AutomaticDepositMadeEvent_SecondDepositAfterFirst_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
        {
            EstateId = TestData.EstateId,
            MerchantId = TestData.MerchantId,
            MerchantName = TestData.MerchantName,
            Balance = TestData.AutomaticDepositMadeEvent.Amount,
            AvailableBalance = TestData.AutomaticDepositMadeEvent.Amount,
            LastDeposit = TestData.AutomaticDepositMadeEvent.DepositDateTime.AddDays(-1),
            DepositCount = 1,
            TotalDeposited = TestData.AutomaticDepositMadeEvent.Amount,
        };

        AutomaticDepositMadeEvent @event = TestData.AutomaticDepositMadeEvent;

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount + @event.Amount);
        newState.Balance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount + @event.Amount);
        newState.DepositCount.ShouldBe(2);
        newState.TotalDeposited.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount + @event.Amount);
        newState.LastDeposit.ShouldBe(TestData.AutomaticDepositMadeEvent.DepositDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_AutomaticDepositMadeEvent_SecondDepositBeforeFirst_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        state = state with
        {
            EstateId = TestData.EstateId,
            MerchantId = TestData.MerchantId,
            MerchantName = TestData.MerchantName,
            Balance = TestData.AutomaticDepositMadeEvent.Amount,
            AvailableBalance = TestData.AutomaticDepositMadeEvent.Amount,
            LastDeposit = TestData.AutomaticDepositMadeEvent.DepositDateTime.AddDays(1),
            DepositCount = 1,
            TotalDeposited = TestData.AutomaticDepositMadeEvent.Amount,
        };

        AutomaticDepositMadeEvent @event = new AutomaticDepositMadeEvent(TestData.AutomaticDepositMadeEvent.AggregateId,
                                                                         TestData.AutomaticDepositMadeEvent.EstateId,
                                                                         TestData.AutomaticDepositMadeEvent.DepositId,
                                                                         TestData.AutomaticDepositMadeEvent.Reference,
                                                                         TestData.AutomaticDepositMadeEvent.DepositDateTime.AddDays(-1),
                                                                         TestData.AutomaticDepositMadeEvent.Amount);

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount + @event.Amount);
        newState.Balance.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount + @event.Amount);
        newState.DepositCount.ShouldBe(2);
        newState.TotalDeposited.ShouldBe(TestData.AutomaticDepositMadeEvent.Amount + @event.Amount);
        newState.LastDeposit.ShouldBe(state.LastDeposit);
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
                    Balance = 100.00m,
                    StartedTransactionCount = 0
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
                    Balance = 100.00m,
                    CompletedTransactionCount = 0,
                    SaleCount = 0,
                    AuthorisedSales = 0
                };
        TransactionHasBeenCompletedEvent @event = TestData.GetTransactionHasBeenCompletedEvent(true, TestData.TransactionAmount);

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance);
        newState.Balance.ShouldBe(state.Balance - @event.TransactionAmount.GetValueOrDefault(0));
        newState.AuthorisedSales.ShouldBe(@event.TransactionAmount.GetValueOrDefault(0));
        newState.SaleCount.ShouldBe(1);
        newState.CompletedTransactionCount.ShouldBe(1);
        newState.LastSale.ShouldBe(@event.CompletedDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_TransactionHasCompletedEvent_SecondSaleAfterFirst_IsAuthorised_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        TransactionHasBeenCompletedEvent @event = TestData.GetTransactionHasBeenCompletedEvent(true, TestData.TransactionAmount);
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 75.00m,
                    Balance = 100.00m,
                    LastSale = @event.CompletedDateTime.AddDays(-1),
                    CompletedTransactionCount = 1,
                    SaleCount = 1,
                    AuthorisedSales = @event.TransactionAmount.GetValueOrDefault(0)
        };

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance);
        newState.Balance.ShouldBe(state.Balance - @event.TransactionAmount.GetValueOrDefault(0));
        newState.AuthorisedSales.ShouldBe(@event.TransactionAmount.GetValueOrDefault(0) * 2);
        newState.SaleCount.ShouldBe(2);
        newState.CompletedTransactionCount.ShouldBe(2);
        newState.LastSale.ShouldBe(@event.CompletedDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_TransactionHasCompletedEvent_SecondSaleBeforeFirst_IsAuthorised_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        TransactionHasBeenCompletedEvent @event = TestData.GetTransactionHasBeenCompletedEvent(true, TestData.TransactionAmount);
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 75.00m,
                    Balance = 100.00m,
                    LastSale = @event.CompletedDateTime.AddDays(1),
                    CompletedTransactionCount = 1,
                    SaleCount = 1,
                    AuthorisedSales = @event.TransactionAmount.GetValueOrDefault(0)
        };

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance);
        newState.Balance.ShouldBe(state.Balance - @event.TransactionAmount.GetValueOrDefault(0));
        newState.AuthorisedSales.ShouldBe(@event.TransactionAmount.GetValueOrDefault(0) * 2);
        newState.SaleCount.ShouldBe(2);
        newState.CompletedTransactionCount.ShouldBe(2);
        newState.LastSale.ShouldBe(state.LastSale);
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
                    Balance = 100.00m,
                    CompletedTransactionCount = 1,
                    SaleCount = 1,
                    DeclinedSales = 0
        };
        TransactionHasBeenCompletedEvent @event = TestData.GetTransactionHasBeenCompletedEvent(false, TestData.TransactionAmount);

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance + @event.TransactionAmount.GetValueOrDefault(0));
        newState.Balance.ShouldBe(state.Balance);
        newState.DeclinedSales.ShouldBe(@event.TransactionAmount.GetValueOrDefault(0));
        newState.SaleCount.ShouldBe(2);
        newState.CompletedTransactionCount.ShouldBe(2);
        newState.LastSale.ShouldBe(@event.CompletedDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_TransactionHasCompletedEvent_SecondSaleAfterFirst_IsNotAuthorised_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        TransactionHasBeenCompletedEvent @event = TestData.GetTransactionHasBeenCompletedEvent(false, TestData.TransactionAmount);
        state = state with
        {
            EstateId = TestData.EstateId,
            MerchantId = TestData.MerchantId,
            MerchantName = TestData.MerchantName,
            AvailableBalance = 75.00m,
            Balance = 100.00m,
            LastSale = @event.CompletedDateTime.AddDays(-1),
            CompletedTransactionCount = 1,
            SaleCount = 1,
            DeclinedSales = @event.TransactionAmount.GetValueOrDefault(0)
        };

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance + @event.TransactionAmount.GetValueOrDefault(0));
        newState.Balance.ShouldBe(state.Balance);
        newState.DeclinedSales.ShouldBe(@event.TransactionAmount.GetValueOrDefault(0) * 2);
        newState.SaleCount.ShouldBe(2);
        newState.CompletedTransactionCount.ShouldBe(2);
        newState.LastSale.ShouldBe(@event.CompletedDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_TransactionHasCompletedEvent_SecondSaleBeforeFirst_IsNotAuthorised_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        TransactionHasBeenCompletedEvent @event = TestData.GetTransactionHasBeenCompletedEvent(false, TestData.TransactionAmount);
        state = state with
        {
            EstateId = TestData.EstateId,
            MerchantId = TestData.MerchantId,
            MerchantName = TestData.MerchantName,
            AvailableBalance = 75.00m,
            Balance = 100.00m,
            LastSale = @event.CompletedDateTime.AddDays(1),
            CompletedTransactionCount = 1,
            SaleCount = 1,
            DeclinedSales = @event.TransactionAmount.GetValueOrDefault(0)
        };

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance + @event.TransactionAmount.GetValueOrDefault(0));
        newState.Balance.ShouldBe(state.Balance);
        newState.DeclinedSales.ShouldBe(@event.TransactionAmount.GetValueOrDefault(0) * 2);
        newState.SaleCount.ShouldBe(2);
        newState.CompletedTransactionCount.ShouldBe(2);
        newState.LastSale.ShouldBe(state.LastSale);
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
        newState.LastFee.ShouldBe(@event.FeeCalculatedDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_MerchantFeeAddedToTransactionEvent_SecondFeeAfterFirst_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantFeeAddedToTransactionEvent @event = TestData.GetMerchantFeeAddedToTransactionEvent(0.25m);
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 75.00m,
                    Balance = 75.00m,
                    FeeCount = 1,
                    LastFee = @event.FeeCalculatedDateTime.AddDays(-1),
                    ValueOfFees = @event.CalculatedValue
        };

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance + @event.CalculatedValue);
        newState.Balance.ShouldBe(state.Balance + @event.CalculatedValue);
        newState.ValueOfFees.ShouldBe(@event.CalculatedValue * 2);
        newState.FeeCount.ShouldBe(2);
        newState.LastFee.ShouldBe(@event.FeeCalculatedDateTime);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_MerchantFeeAddedToTransactionEvent_SecondFeeBeforeFirst_EventIsHandled()
    {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantFeeAddedToTransactionEvent @event = TestData.GetMerchantFeeAddedToTransactionEvent(0.25m);
        state = state with
                {
                    EstateId = TestData.EstateId,
                    MerchantId = TestData.MerchantId,
                    MerchantName = TestData.MerchantName,
                    AvailableBalance = 75.00m,
                    Balance = 75.00m,
                    FeeCount = 1,
                    LastFee = @event.FeeCalculatedDateTime.AddDays(1),
                    ValueOfFees = @event.CalculatedValue
                };
        

        MerchantBalanceState newState = await projection.Handle(state, @event, CancellationToken.None);

        newState.AvailableBalance.ShouldBe(state.AvailableBalance + @event.CalculatedValue);
        newState.Balance.ShouldBe(state.Balance + @event.CalculatedValue);
        newState.ValueOfFees.ShouldBe(@event.CalculatedValue * 2);
        newState.FeeCount.ShouldBe(2);
        newState.LastFee.ShouldBe(state.LastFee);
    }

    [Fact]
    public async Task MerchantBalanceProjection_Handle_EventsOutOfSequence_EventsAreHandled() {
        MerchantBalanceProjection projection = new MerchantBalanceProjection();
        MerchantBalanceState state = new MerchantBalanceState();

        MerchantCreatedEvent merchantCreatedEvent = TestData.MerchantCreatedEvent;
        ManualDepositMadeEvent manualDepositMadeEvent = TestData.ManualDepositMadeEvent;
        TransactionHasStartedEvent transactionHasStartedEvent = TestData.GetTransactionHasStartedEvent(TestData.TransactionAmount);
        TransactionHasBeenCompletedEvent transactionHasBeenCompleteEvent = TestData.GetTransactionHasBeenCompletedEvent(true, TestData.TransactionAmount);
        MerchantFeeAddedToTransactionEvent merchantFeeAddedToTransactionEvent = TestData.GetMerchantFeeAddedToTransactionEvent(0.25m);

        MerchantBalanceState newState = await projection.Handle(state,merchantCreatedEvent , CancellationToken.None);
        newState = await projection.Handle(newState, merchantFeeAddedToTransactionEvent, CancellationToken.None);
        newState = await projection.Handle(newState, transactionHasStartedEvent, CancellationToken.None);
        newState = await projection.Handle(newState, manualDepositMadeEvent, CancellationToken.None);
        newState = await projection.Handle(newState, transactionHasBeenCompleteEvent, CancellationToken.None);
        
        newState.EstateId.ShouldBe(merchantCreatedEvent.EstateId);
        newState.MerchantId.ShouldBe(merchantCreatedEvent.MerchantId);
        newState.MerchantName.ShouldBe(merchantCreatedEvent.MerchantName);
        newState.AvailableBalance.ShouldBe(manualDepositMadeEvent.Amount  - transactionHasBeenCompleteEvent.TransactionAmount.GetValueOrDefault() + merchantFeeAddedToTransactionEvent.CalculatedValue);
        newState.Balance.ShouldBe(manualDepositMadeEvent.Amount - transactionHasBeenCompleteEvent.TransactionAmount.GetValueOrDefault() + merchantFeeAddedToTransactionEvent.CalculatedValue);
        newState.ValueOfFees.ShouldBe(merchantFeeAddedToTransactionEvent.CalculatedValue);
        newState.FeeCount.ShouldBe(1);

    }
}