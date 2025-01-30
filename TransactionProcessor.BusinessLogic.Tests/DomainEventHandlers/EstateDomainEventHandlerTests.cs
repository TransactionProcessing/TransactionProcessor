using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.Estate.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class EstateDomainEventHandlerTests
{
    #region Methods

    private Mock<ITransactionProcessorReadModelRepository> EstateReportingRepository;

    private EstateDomainEventHandler DomainEventHandler;

    public EstateDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        this.EstateReportingRepository = new Mock<ITransactionProcessorReadModelRepository>();

        this.DomainEventHandler = new EstateDomainEventHandler(this.EstateReportingRepository.Object);
    }
    [Fact]
    public void EstateDomainEventHandler_EstateCreatedEvent_EventIsHandled()
    {
        EstateCreatedEvent estateCreatedEvent = TestData.DomainEvents.EstateCreatedEvent;
        this.EstateReportingRepository
            .Setup(r => r.CreateReadModel(It.IsAny<EstateCreatedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(estateCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public async Task EstateDomainEventHandler_EstateCreatedEvent_CreateReadModelFailed_EventIsHandled()
    {
        EstateCreatedEvent estateCreatedEvent = TestData.DomainEvents.EstateCreatedEvent;
        this.EstateReportingRepository
            .Setup(r => r.CreateReadModel(It.IsAny<EstateCreatedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure);

        var result = await this.DomainEventHandler.Handle(estateCreatedEvent, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void EstateDomainEventHandler_EstateReferenceAllocatedEvent_EventIsHandled()
    {
        EstateReferenceAllocatedEvent estateReferenceAllocatedEvent = TestData.DomainEvents.EstateReferenceAllocatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(estateReferenceAllocatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void EstateDomainEventHandler_SecurityUserAddedEvent_EventIsHandled()
    {
        SecurityUserAddedToEstateEvent securityUserAddedEvent = TestData.DomainEvents.EstateSecurityUserAddedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(securityUserAddedEvent, CancellationToken.None); });
    }

    #endregion
}