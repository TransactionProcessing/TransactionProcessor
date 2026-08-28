using Moq;
using Shared.Logger;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class EstateDomainEventHandlerTests
{
    #region Methods

    private Mock<ITransactionProcessorReadModelRepository> EstateReportingRepository;

    private ReadModelDomainEventHandler DomainEventHandler;

    public EstateDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        StringSerialiser.Initialise(new Shared.Serialisation.SystemTextJsonSerializer(new System.Text.Json.JsonSerializerOptions()));
        this.EstateReportingRepository = new Mock<ITransactionProcessorReadModelRepository>();

        this.DomainEventHandler = new ReadModelDomainEventHandler(this.EstateReportingRepository.Object);
    }
    [Fact]
    public void EstateDomainEventHandler_EstateCreatedEvent_EventIsHandled()
    {
        EstateDomainEvents.EstateCreatedEvent estateCreatedEvent = TestData.DomainEvents.EstateCreatedEvent;
        this.EstateReportingRepository
            .Setup(r => r.CreateReadModel(It.IsAny<EstateDomainEvents.EstateCreatedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(estateCreatedEvent, TestContext.Current.CancellationToken); });
    }

    [Fact]
    public async Task EstateDomainEventHandler_EstateCreatedEvent_CreateReadModelFailed_EventIsHandled()
    {
        EstateDomainEvents.EstateCreatedEvent estateCreatedEvent = TestData.DomainEvents.EstateCreatedEvent;
        this.EstateReportingRepository
            .Setup(r => r.CreateReadModel(It.IsAny<EstateDomainEvents.EstateCreatedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure);

        Result result = await this.DomainEventHandler.Handle(estateCreatedEvent, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public void EstateDomainEventHandler_EstateReferenceAllocatedEvent_EventIsHandled()
    {
        EstateDomainEvents.EstateReferenceAllocatedEvent estateReferenceAllocatedEvent = TestData.DomainEvents.EstateReferenceAllocatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(estateReferenceAllocatedEvent, TestContext.Current.CancellationToken); });
    }

    [Fact]
    public void EstateDomainEventHandler_SecurityUserAddedEvent_EventIsHandled()
    {
        EstateDomainEvents.SecurityUserAddedToEstateEvent securityUserAddedEvent = TestData.DomainEvents.EstateSecurityUserAddedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(securityUserAddedEvent, TestContext.Current.CancellationToken); });
    }

    #endregion
}
