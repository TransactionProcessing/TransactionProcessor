using Shared.Logger;
using SimpleResults;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.ProjectionEngine.Models;

namespace TransactionProcessor.ProjectionEngine.Tests;

using Dispatchers;
using Microsoft.Extensions.Configuration;
using Projections;
using Repository;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.General;
using Shouldly;
using State;

public class ProjectionHandlerTests{

    public ProjectionHandlerTests(){
        Shared.Logger.Logger.Initialise(Shared.Logger.NullLogger.Instance);
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
    }

    [Fact]
    public async Task ProjectionHandler_Handle_NullEvent_EventHandled(){
        TestProjectionStateRepository repo = new();
        TestProjection projection = new();
        TestStateDispatcher stateDispatcher = new();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo,projection,
                                                                                                                                     stateDispatcher);
        var result = await ph.Handle(null, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_EventNotSupported_EventHandled()
    {
        TestProjectionStateRepository repo = new();
        TestProjection projection = new();
        TestStateDispatcher stateDispatcher = new();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo, projection,
                                                                                                                                     stateDispatcher);

        projection.ShouldHandle = false;
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_StateNotFoundInRepository_EventHandled()
    {
        TestProjectionStateRepository repo = new();
        TestProjection projection = new();
        TestStateDispatcher stateDispatcher = new();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo, projection,
                                                                                                                                     stateDispatcher);

        MerchantBalanceState state = new MerchantBalanceState();
        projection.ShouldHandle = true;
        projection.HandleFunc = (_, _, _) => state;
        repo.LoadResult = Result.Success(state);
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_StateFoundInRepository_NoChanges_EventHandled()
    {
        TestProjectionStateRepository repo = new();
        TestProjection projection = new();
        TestStateDispatcher stateDispatcher = new();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo, projection,
                                                                                                                                     stateDispatcher);

        MerchantBalanceState state = new MerchantBalanceState();

        projection.ShouldHandle = true;
        projection.HandleFunc = (_, _, _) => state.InitialiseBalances();
            
        repo.LoadResult = state;

        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_StateFoundInRepository_ChangesMade_EventHandled()
    {
        TestProjectionStateRepository repo = new();
        TestProjection projection = new();
        TestStateDispatcher stateDispatcher = new();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo, projection,
                                                                                                                                     stateDispatcher);
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantBalanceState newState = new MerchantBalanceState();
        newState = newState.HandleMerchantCreated(TestData.MerchantCreatedEvent);
        projection.ShouldHandle = true;
        projection.HandleFunc = (_, _, _) => newState;
            
        repo.LoadResult = state;
        repo.SaveResult = Result.Success(state);

        stateDispatcher.DispatchResult = Result.Success();
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_LoadFails_FailedResult()
    {
        TestProjectionStateRepository repo = new();
        TestProjection projection = new();
        TestStateDispatcher stateDispatcher = new();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo, projection,
            stateDispatcher);
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantBalanceState newState = new MerchantBalanceState();
        newState = newState.HandleMerchantCreated(TestData.MerchantCreatedEvent);
        projection.ShouldHandle = true;
        projection.HandleFunc = (_, _, _) => newState;

        repo.LoadResult = Result.Failure();
        //repo.SaveResult = Result.Success(state);

        //stateDispatcher.DispatchResult = Result.Success();
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_SaveFails_FailedResult()
    {
        TestProjectionStateRepository repo = new();
        TestProjection projection = new();
        TestStateDispatcher stateDispatcher = new();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo, projection,
            stateDispatcher);
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantBalanceState newState = new MerchantBalanceState();
        newState = newState.HandleMerchantCreated(TestData.MerchantCreatedEvent);
        projection.ShouldHandle = true;
        projection.HandleFunc = (_, _, _) => newState;

        repo.LoadResult = state;
        repo.SaveResult = Result.Failure();

        //stateDispatcher.DispatchResult = Result.Success();
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_DispatchFails_FailedResult()
    {
        TestProjectionStateRepository repo = new();
        TestProjection projection = new();
        TestStateDispatcher stateDispatcher = new();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo, projection,
            stateDispatcher);
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantBalanceState newState = new MerchantBalanceState();
        newState = newState.HandleMerchantCreated(TestData.MerchantCreatedEvent);
        projection.ShouldHandle = true;
        projection.HandleFunc = (_, _, _) => newState;

        repo.LoadResult = state;
        repo.SaveResult = Result.Success(state);

        stateDispatcher.DispatchResult = Result.Failure();
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }
}

public class VoucherStateDispatcherTests {
    private readonly IStateDispatcher<VoucherState> Dispatcher;

    public VoucherStateDispatcherTests() {
        this.Dispatcher = new VoucherStateDispatcher();
    }

    [Fact]
    public async Task MerchantBalanceStateDispatcher_TransactionHasBeenCompletedEvent_NotAuthorised_ResultSuccessful()
    {
        VoucherState state = new();

        IDomainEvent domainEvent = TestData.TransactionHasBeenCompletedEvent;
        
        var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
}

public class MerchantBalanceStateDispatcherTests
{
    private readonly IStateDispatcher<MerchantBalanceState> Dispatcher;
    private readonly TestReadRepository Repository;

    public MerchantBalanceStateDispatcherTests() {
        this.Repository = new TestReadRepository();
        this.Dispatcher = new MerchantBalanceStateDispatcher(this.Repository);
        Logger.Initialise(new NullLogger());
    }

    [Theory]
    [InlineData(typeof(MerchantDomainEvents.MerchantCreatedEvent))]
    [InlineData(typeof(MerchantDomainEvents.ManualDepositMadeEvent))]
    [InlineData(typeof(MerchantDomainEvents.AutomaticDepositMadeEvent))]
    [InlineData(typeof(MerchantDomainEvents.WithdrawalMadeEvent))]
    [InlineData(typeof(TransactionDomainEvents.TransactionHasBeenCompletedEvent))]
    [InlineData(typeof(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent))]
    public async Task MerchantBalanceStateDispatcher_EventIsDispatched_ResultSuccessful(Type type) {
        MerchantBalanceState state = new();

        this.Repository.AddResult = Result.Success();

        IDomainEvent domainEvent = type.Name switch {
            nameof(MerchantDomainEvents.MerchantCreatedEvent) => TestData.MerchantCreatedEvent,
            nameof(MerchantDomainEvents.ManualDepositMadeEvent) => TestData.ManualDepositMadeEvent,
            nameof(MerchantDomainEvents.AutomaticDepositMadeEvent) => TestData.AutomaticDepositMadeEvent,
            nameof(MerchantDomainEvents.WithdrawalMadeEvent) => TestData.WithdrawalMadeEvent,
            nameof(TransactionDomainEvents.TransactionHasBeenCompletedEvent) => TestData.TransactionHasBeenCompletedEvent,
            nameof(TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent) => TestData.SettledMerchantFeeAddedToTransactionEvent(DateTime.Now),
            _ => null
        };

        if (domainEvent != null) {
            var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            this.Repository.AddCalls.ShouldBe(1);
        }
    }

    [Fact]
    public async Task MerchantBalanceStateDispatcher_TransactionHasBeenCompletedEvent_NotAuthorised_ResultSuccessful()
    {
        MerchantBalanceState state = new();

        this.Repository.AddResult = Result.Success();

        IDomainEvent domainEvent = TestData.TransactionHasBeenCompletedEvent with {
                IsAuthorised = false
            };
            var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            this.Repository.AddCalls.ShouldBe(0);
    }

    [Fact]
    public async Task MerchantBalanceStateDispatcher_TransactionHasBeenCompletedEvent_AmountIsZero_ResultSuccessful()
    {
        MerchantBalanceState state = new();

        this.Repository.AddResult = Result.Success();

        IDomainEvent domainEvent = TestData.TransactionHasBeenCompletedEvent with
        {
            TransactionAmount = 0
        };
        var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        this.Repository.AddCalls.ShouldBe(0);
    }

    [Fact]
    public async Task MerchantBalanceStateDispatcher_TransactionHasBeenCompletedEvent_AmountIsNegative_ResultSuccessful()
    {
        MerchantBalanceState state = new();

        this.Repository.AddResult = Result.Success();

        IDomainEvent domainEvent = TestData.TransactionHasBeenCompletedEvent with
        {
            TransactionAmount = -1
        };
        var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        this.Repository.AddCalls.ShouldBe(1);
    }

}

internal sealed class TestProjectionStateRepository : IProjectionStateRepository<MerchantBalanceState>
{
    public Result<MerchantBalanceState> LoadResult { get; set; } = new();
    public Result<MerchantBalanceState> SaveResult { get; set; } = new();

    public Task<Result<MerchantBalanceState>> Load(IDomainEvent @event, CancellationToken cancellationToken) => Task.FromResult(LoadResult);
    public Task<Result<MerchantBalanceState>> Load(Guid estateId, Guid stateId, CancellationToken cancellationToken) => Task.FromResult(LoadResult);
    public Task<Result<MerchantBalanceState>> Save(MerchantBalanceState state, IDomainEvent @event, CancellationToken cancellationToken) => Task.FromResult(SaveResult);
}

internal sealed class TestProjection : IProjection<MerchantBalanceState>
{
    public bool ShouldHandle { get; set; }
    public Func<MerchantBalanceState, IDomainEvent, CancellationToken, MerchantBalanceState> HandleFunc { get; set; } = (state, _, _) => state;

    public Task<MerchantBalanceState> Handle(MerchantBalanceState state, IDomainEvent domainEvent, CancellationToken cancellationToken) =>
        Task.FromResult(HandleFunc(state, domainEvent, cancellationToken));

    public bool ShouldIHandleEvent(IDomainEvent domainEvent) => ShouldHandle;
}

internal sealed class TestStateDispatcher : IStateDispatcher<MerchantBalanceState>
{
    public Result DispatchResult { get; set; } = Result.Success();
    public Task<Result> Dispatch(MerchantBalanceState state, IDomainEvent @event, CancellationToken cancellationToken) => Task.FromResult(DispatchResult);
}

internal sealed class TestReadRepository : ITransactionProcessorReadRepository
{
    public Result AddResult { get; set; } = Result.Success();
    public int AddCalls { get; private set; }

    public Task<Result> AddMerchantBalanceChangedEntry(MerchantBalanceChangedEntry entry, CancellationToken cancellationToken)
    {
        AddCalls++;
        return Task.FromResult(AddResult);
    }

    public Task<Result<List<MerchantBalanceChangedEntry>>> GetMerchantBalanceHistory(Guid estateId, Guid merchantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken) =>
        Task.FromResult(new Result<List<MerchantBalanceChangedEntry>>());
}
