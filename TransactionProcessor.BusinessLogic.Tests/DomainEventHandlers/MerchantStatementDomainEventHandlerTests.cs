using SimpleResults;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.Testing;

namespace EstateManagement.BusinessLogic.Tests.EventHandling;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using Shared.Logger;
using Shouldly;
using Xunit;

public class MerchantStatementDomainEventHandlerTests
{
    private Mock<IMediator> Mediator;

    private MerchantStatementDomainEventHandler DomainEventHandler;
    public MerchantStatementDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        this.Mediator = new Mock<IMediator>();
        this.DomainEventHandler = new MerchantStatementDomainEventHandler(this.Mediator.Object);
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementGeneratedEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Should.NotThrow(async () =>
                        {
                            await this.DomainEventHandler.Handle(TestData.DomainEvents.StatementGeneratedEvent, CancellationToken.None);
                        });
        this.Mediator.Verify(m=> m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        Should.NotThrow(async () =>
                        {
                            await this.DomainEventHandler.Handle(TestData.TransactionHasBeenCompletedEvent, CancellationToken.None);
                        });
        this.Mediator.Verify(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}