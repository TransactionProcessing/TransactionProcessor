using System.Threading;
using Moq;
using Shared.Logger;
using Shouldly;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.Estate.DomainEvents;
using TransactionProcessor.Operator.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class OperatorDomainEventHandlerTests
{
    private Mock<ITransactionProcessorReadModelRepository> EstateReportingRepository;
    private OperatorDomainEventHandler DomainEventHandler;

    public OperatorDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        this.EstateReportingRepository = new Mock<ITransactionProcessorReadModelRepository>();
        this.DomainEventHandler = new OperatorDomainEventHandler(this.EstateReportingRepository.Object);
    }

    [Fact]
    public void OperatorDomainEventHandler_OperatorCreatedEvent_EventIsHandled()
    {
        OperatorCreatedEvent operatorCreatedEvent = TestData.DomainEvents.OperatorCreatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void OperatorDomainEventHandler_OperatorNameUpdatedEvent_EventIsHandled()
    {
        OperatorNameUpdatedEvent operatorCreatedEvent = TestData.DomainEvents.OperatorNameUpdatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void OperatorDomainEventHandler_OperatorRequireCustomMerchantNumberChangedEvent_EventIsHandled()
    {
        OperatorRequireCustomMerchantNumberChangedEvent operatorCreatedEvent = TestData.DomainEvents.OperatorRequireCustomMerchantNumberChangedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void OperatorDomainEventHandler_OperatorRequireCustomTerminalNumberChangedEvent_EventIsHandled()
    {
        OperatorRequireCustomTerminalNumberChangedEvent operatorCreatedEvent = TestData.DomainEvents.OperatorRequireCustomTerminalNumberChangedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(operatorCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void OperatorDomainEventHandler_EstateCreatedEvent_EventIsHandled()
    {
        EstateCreatedEvent domainEvent = TestData.DomainEvents.EstateCreatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None); });
    }
}