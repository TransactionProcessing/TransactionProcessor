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

public class ContractDomainEventHandlerTests
{
    #region Methods

    private Mock<ITransactionProcessorReadModelRepository> EstateReportingRepository;

    private ContractDomainEventHandler DomainEventHandler;
    public ContractDomainEventHandlerTests() {
        Logger.Initialise(NullLogger.Instance);
        this.EstateReportingRepository= new Mock<ITransactionProcessorReadModelRepository>();
        this.DomainEventHandler = new ContractDomainEventHandler(this.EstateReportingRepository.Object);
    }
        
    [Fact]
    public void ContractDomainEventHandler_ContractCreatedEvent_EventIsHandled()
    {
        ContractDomainEvents.ContractCreatedEvent contractCreatedEvent = TestData.DomainEvents.ContractCreatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(contractCreatedEvent, CancellationToken.None); });
    }

    [Fact]
    public void ContractDomainEventHandler_FixedValueProductAddedToContractEvent_EventIsHandled()
    {
        ContractDomainEvents.FixedValueProductAddedToContractEvent fixedValueProductAddedToContractEvent = TestData.DomainEvents.FixedValueProductAddedToContractEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(fixedValueProductAddedToContractEvent, CancellationToken.None); });
    }

    [Fact]
    public void ContractDomainEventHandler_TransactionFeeForProductAddedToContractEvent_EventIsHandled()
    {
        ContractDomainEvents.TransactionFeeForProductAddedToContractEvent transactionFeeForProductAddedToContractEvent = TestData.DomainEvents.TransactionFeeForProductAddedToContractEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(transactionFeeForProductAddedToContractEvent, CancellationToken.None); });
    }

    [Fact]
    public void ContractDomainEventHandler_TransactionFeeForProductDisabledEvent_EventIsHandled()
    {
        ContractDomainEvents.TransactionFeeForProductDisabledEvent transactionFeeForProductDisabledEvent = TestData.DomainEvents.TransactionFeeForProductDisabledEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(transactionFeeForProductDisabledEvent, CancellationToken.None); });
    }

    [Fact]
    public void ContractDomainEventHandler_VariableValueProductAddedToContractEvent_EventIsHandled()
    {
        ContractDomainEvents.VariableValueProductAddedToContractEvent variableValueProductAddedToContractEvent = TestData.DomainEvents.VariableValueProductAddedToContractEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(variableValueProductAddedToContractEvent, CancellationToken.None); });
    }

    #endregion
}