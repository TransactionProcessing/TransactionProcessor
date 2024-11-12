using SimpleResults;

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
}