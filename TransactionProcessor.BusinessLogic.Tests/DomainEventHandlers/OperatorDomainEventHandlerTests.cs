using System.Threading;
using Moq;
using Shared.Logger;
using Shouldly;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class OperatorDomainEventHandlerTests
{
    private Mock<ITransactionProcessorReadModelRepository> EstateReportingRepository;
    private ReadModelDomainEventHandler DomainEventHandler;

    public OperatorDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        this.EstateReportingRepository = new Mock<ITransactionProcessorReadModelRepository>();
        this.DomainEventHandler = new ReadModelDomainEventHandler(this.EstateReportingRepository.Object);
    }

    [Fact]
    public void OperatorDomainEventHandler_OperatorCreatedEvent_EventIsHandled()
    {
        OperatorDomainEvents.OperatorCreatedEvent operatorCreatedEvent = TestData.DomainEvents.OperatorCreatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void OperatorDomainEventHandler_OperatorNameUpdatedEvent_EventIsHandled()
    {
        OperatorDomainEvents.OperatorNameUpdatedEvent operatorCreatedEvent = TestData.DomainEvents.OperatorNameUpdatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void OperatorDomainEventHandler_OperatorRequireCustomMerchantNumberChangedEvent_EventIsHandled()
    {
        OperatorDomainEvents.OperatorRequireCustomMerchantNumberChangedEvent operatorCreatedEvent = TestData.DomainEvents.OperatorRequireCustomMerchantNumberChangedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void OperatorDomainEventHandler_OperatorRequireCustomTerminalNumberChangedEvent_EventIsHandled()
    {
        OperatorDomainEvents.OperatorRequireCustomTerminalNumberChangedEvent operatorCreatedEvent = TestData.DomainEvents.OperatorRequireCustomTerminalNumberChangedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorCreatedEvent, CancellationToken.None); });
    }
}