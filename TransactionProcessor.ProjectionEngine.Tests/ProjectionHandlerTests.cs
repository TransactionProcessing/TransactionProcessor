using EstateManagement.Merchant.DomainEvents;
using SimpleResults;
using TransactionProcessor.ProjectionEngine.Models;
using TransactionProcessor.Transaction.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.Tests;

using Dispatchers;
using Microsoft.Extensions.Configuration;
using Moq;
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
        Mock<IProjectionStateRepository<MerchantBalanceState>> repo = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        Mock<IProjection<MerchantBalanceState>> projection = new Mock<IProjection<MerchantBalanceState>>();
        Mock<IStateDispatcher<MerchantBalanceState>> stateDispatcher = new Mock<IStateDispatcher<MerchantBalanceState>>();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo.Object,projection.Object,
                                                                                                                                     stateDispatcher.Object);
        var result = await ph.Handle(null, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_EventNotSupported_EventHandled()
    {
        Mock<IProjectionStateRepository<MerchantBalanceState>> repo = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        Mock<IProjection<MerchantBalanceState>> projection = new Mock<IProjection<MerchantBalanceState>>();
        Mock<IStateDispatcher<MerchantBalanceState>> stateDispatcher = new Mock<IStateDispatcher<MerchantBalanceState>>();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo.Object, projection.Object,
                                                                                                                                     stateDispatcher.Object);

        projection.Setup(p=> p.ShouldIHandleEvent(It.IsAny<IDomainEvent>())).Returns(false);
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_StateNotFoundInRepository_EventHandled()
    {
        Mock<IProjectionStateRepository<MerchantBalanceState>> repo = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        Mock<IProjection<MerchantBalanceState>> projection = new Mock<IProjection<MerchantBalanceState>>();
        Mock<IStateDispatcher<MerchantBalanceState>> stateDispatcher = new Mock<IStateDispatcher<MerchantBalanceState>>();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo.Object, projection.Object,
                                                                                                                                     stateDispatcher.Object);

        MerchantBalanceState state = new MerchantBalanceState();
        projection.Setup(p => p.ShouldIHandleEvent(It.IsAny<IDomainEvent>())).Returns(true);
        projection.Setup(p =>
                p.Handle(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(state));
        repo.Setup(r => r.Load(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(state));
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_StateFoundInRepository_NoChanges_EventHandled()
    {
        Mock<IProjectionStateRepository<MerchantBalanceState>> repo = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        Mock<IProjection<MerchantBalanceState>> projection = new Mock<IProjection<MerchantBalanceState>>();
        Mock<IStateDispatcher<MerchantBalanceState>> stateDispatcher = new Mock<IStateDispatcher<MerchantBalanceState>>();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo.Object, projection.Object,
                                                                                                                                     stateDispatcher.Object);

        MerchantBalanceState state = new MerchantBalanceState();

        projection.Setup(p => p.ShouldIHandleEvent(It.IsAny<IDomainEvent>())).Returns(true);
        projection.Setup(p => p.Handle(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(state.InitialiseBalances);
            
        repo.Setup(r => r.Load(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(state);

        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_StateFoundInRepository_ChangesMade_EventHandled()
    {
        Mock<IProjectionStateRepository<MerchantBalanceState>> repo = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        Mock<IProjection<MerchantBalanceState>> projection = new Mock<IProjection<MerchantBalanceState>>();
        Mock<IStateDispatcher<MerchantBalanceState>> stateDispatcher = new Mock<IStateDispatcher<MerchantBalanceState>>();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo.Object, projection.Object,
                                                                                                                                     stateDispatcher.Object);
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantBalanceState newState = new MerchantBalanceState();
        newState = newState.HandleMerchantCreated(TestData.MerchantCreatedEvent);
        projection.Setup(p => p.ShouldIHandleEvent(It.IsAny<IDomainEvent>())).Returns(true);
        projection.Setup(p => p.Handle(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(newState));
            
        repo.Setup(r => r.Load(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(state);
        repo.Setup(r => r.Save(It.IsAny<MerchantBalanceState>(),It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(state));

        stateDispatcher.Setup(d => d.Dispatch(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_LoadFails_FailedResult()
    {
        Mock<IProjectionStateRepository<MerchantBalanceState>> repo = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        Mock<IProjection<MerchantBalanceState>> projection = new Mock<IProjection<MerchantBalanceState>>();
        Mock<IStateDispatcher<MerchantBalanceState>> stateDispatcher = new Mock<IStateDispatcher<MerchantBalanceState>>();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo.Object, projection.Object,
            stateDispatcher.Object);
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantBalanceState newState = new MerchantBalanceState();
        newState = newState.HandleMerchantCreated(TestData.MerchantCreatedEvent);
        projection.Setup(p => p.ShouldIHandleEvent(It.IsAny<IDomainEvent>())).Returns(true);
        projection.Setup(p => p.Handle(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(newState));

        repo.Setup(r => r.Load(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
        //repo.Setup(r => r.Save(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(state));

        //stateDispatcher.Setup(d => d.Dispatch(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_SaveFails_FailedResult()
    {
        Mock<IProjectionStateRepository<MerchantBalanceState>> repo = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        Mock<IProjection<MerchantBalanceState>> projection = new Mock<IProjection<MerchantBalanceState>>();
        Mock<IStateDispatcher<MerchantBalanceState>> stateDispatcher = new Mock<IStateDispatcher<MerchantBalanceState>>();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo.Object, projection.Object,
            stateDispatcher.Object);
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantBalanceState newState = new MerchantBalanceState();
        newState = newState.HandleMerchantCreated(TestData.MerchantCreatedEvent);
        projection.Setup(p => p.ShouldIHandleEvent(It.IsAny<IDomainEvent>())).Returns(true);
        projection.Setup(p => p.Handle(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(newState));

        repo.Setup(r => r.Load(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(state);
        repo.Setup(r => r.Save(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());

        //stateDispatcher.Setup(d => d.Dispatch(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var result = await ph.Handle(TestData.MerchantCreatedEvent, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ProjectionHandler_Handle_DispatchFails_FailedResult()
    {
        Mock<IProjectionStateRepository<MerchantBalanceState>> repo = new Mock<IProjectionStateRepository<MerchantBalanceState>>();
        Mock<IProjection<MerchantBalanceState>> projection = new Mock<IProjection<MerchantBalanceState>>();
        Mock<IStateDispatcher<MerchantBalanceState>> stateDispatcher = new Mock<IStateDispatcher<MerchantBalanceState>>();
        ProjectionHandler.ProjectionHandler<MerchantBalanceState> ph = new ProjectionHandler.ProjectionHandler<MerchantBalanceState>(repo.Object, projection.Object,
            stateDispatcher.Object);
        MerchantBalanceState state = new MerchantBalanceState();
        MerchantBalanceState newState = new MerchantBalanceState();
        newState = newState.HandleMerchantCreated(TestData.MerchantCreatedEvent);
        projection.Setup(p => p.ShouldIHandleEvent(It.IsAny<IDomainEvent>())).Returns(true);
        projection.Setup(p => p.Handle(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(newState));

        repo.Setup(r => r.Load(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(state);
        repo.Setup(r => r.Save(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(state));

        stateDispatcher.Setup(d => d.Dispatch(It.IsAny<MerchantBalanceState>(), It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);
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
    private readonly Mock<ITransactionProcessorReadRepository> Repository;

    public MerchantBalanceStateDispatcherTests() {
        this.Repository = new Mock<ITransactionProcessorReadRepository>();
        this.Dispatcher = new MerchantBalanceStateDispatcher(this.Repository.Object);
    }

    [Theory]
    [InlineData(typeof(MerchantCreatedEvent))]
    [InlineData(typeof(ManualDepositMadeEvent))]
    [InlineData(typeof(AutomaticDepositMadeEvent))]
    [InlineData(typeof(WithdrawalMadeEvent))]
    [InlineData(typeof(TransactionHasBeenCompletedEvent))]
    [InlineData(typeof(SettledMerchantFeeAddedToTransactionEvent))]
    public async Task MerchantBalanceStateDispatcher_EventIsDispatched_ResultSuccessful(Type type) {
        MerchantBalanceState state = new();

        this.Repository.Setup(t => t.AddMerchantBalanceChangedEntry(It.IsAny<MerchantBalanceChangedEntry>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        IDomainEvent domainEvent = type.Name switch {
            nameof(MerchantCreatedEvent) => TestData.MerchantCreatedEvent,
            nameof(ManualDepositMadeEvent) => TestData.ManualDepositMadeEvent,
            nameof(AutomaticDepositMadeEvent) => TestData.AutomaticDepositMadeEvent,
            nameof(WithdrawalMadeEvent) => TestData.WithdrawalMadeEvent,
            nameof(TransactionHasBeenCompletedEvent) => TestData.TransactionHasBeenCompletedEvent,
            nameof(SettledMerchantFeeAddedToTransactionEvent) => TestData.SettledMerchantFeeAddedToTransactionEvent(DateTime.Now),
            _ => null
        };

        if (domainEvent != null) {
            var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            this.Repository.Verify(t => t.AddMerchantBalanceChangedEntry(It.IsAny<MerchantBalanceChangedEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task MerchantBalanceStateDispatcher_TransactionHasBeenCompletedEvent_NotAuthorised_ResultSuccessful()
    {
        MerchantBalanceState state = new();

        this.Repository.Setup(t => t.AddMerchantBalanceChangedEntry(It.IsAny<MerchantBalanceChangedEntry>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        IDomainEvent domainEvent = TestData.TransactionHasBeenCompletedEvent with {
                IsAuthorised = false
            };
            var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
            result.IsSuccess.ShouldBeTrue();
            this.Repository.Verify(t => t.AddMerchantBalanceChangedEntry(It.IsAny<MerchantBalanceChangedEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MerchantBalanceStateDispatcher_TransactionHasBeenCompletedEvent_AmountIsZero_ResultSuccessful()
    {
        MerchantBalanceState state = new();

        this.Repository.Setup(t => t.AddMerchantBalanceChangedEntry(It.IsAny<MerchantBalanceChangedEntry>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        IDomainEvent domainEvent = TestData.TransactionHasBeenCompletedEvent with
        {
            TransactionAmount = 0
        };
        var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        this.Repository.Verify(t => t.AddMerchantBalanceChangedEntry(It.IsAny<MerchantBalanceChangedEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MerchantBalanceStateDispatcher_TransactionHasBeenCompletedEvent_AmountIsNegative_ResultSuccessful()
    {
        MerchantBalanceState state = new();

        this.Repository.Setup(t => t.AddMerchantBalanceChangedEntry(It.IsAny<MerchantBalanceChangedEntry>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success);

        IDomainEvent domainEvent = TestData.TransactionHasBeenCompletedEvent with
        {
            TransactionAmount = -1
        };
        var result = await this.Dispatcher.Dispatch(state, domainEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        this.Repository.Verify(t => t.AddMerchantBalanceChangedEntry(It.IsAny<MerchantBalanceChangedEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

}