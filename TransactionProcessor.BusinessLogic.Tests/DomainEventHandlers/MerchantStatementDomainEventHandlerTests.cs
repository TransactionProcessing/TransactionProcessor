using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Grpc.Core;
using MediatR;
using Moq;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class MerchantStatementDomainEventHandlerTests
{
    private Mock<IMediator> Mediator;

    private MerchantStatementDomainEventHandler EventHandler;
    public MerchantStatementDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        this.Mediator = new Mock<IMediator>();
        this.EventHandler = new MerchantStatementDomainEventHandler(this.Mediator.Object);
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementGeneratedEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementGeneratedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementGeneratedEvent_Retry_EventIsHandled()
    {
        this.Mediator.SetupSequence(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WrongExpectedVersionException("Stream1", StreamRevision.None, StreamRevision.None))
            .ThrowsAsync(new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementGeneratedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.TransactionHasBeenCompletedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_Retry_EventIsHandled()
    {
        this.Mediator.SetupSequence(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WrongExpectedVersionException("Stream1", StreamRevision.None, StreamRevision.None))
            .ThrowsAsync(new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.TransactionHasBeenCompletedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
}