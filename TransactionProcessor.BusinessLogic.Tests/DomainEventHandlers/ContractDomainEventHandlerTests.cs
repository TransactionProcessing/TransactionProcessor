using TransactionProcessor.BusinessLogic.Tests;
using Imposter.Abstractions;
using Shared.Logger;
using Shared.Serialisation;
using Shouldly;
using System.Text.Json;
using System.Threading;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class ContractDomainEventHandlerTests
{

    #region Methods

    private ITransactionProcessorReadModelRepositoryImposter EstateReportingRepository;

    private ReadModelDomainEventHandler DomainEventHandler;
    public ContractDomainEventHandlerTests() {
        Logger.Initialise(NullLogger.Instance);
        StringSerialiser.Initialise(new Shared.Serialisation.SystemTextJsonSerializer(new System.Text.Json.JsonSerializerOptions()));
        this.EstateReportingRepository= new ITransactionProcessorReadModelRepositoryImposter();
        this.DomainEventHandler = new ReadModelDomainEventHandler(this.EstateReportingRepository.Instance());
    }
        
    [Fact]
    public void ContractDomainEventHandler_ContractCreatedEvent_EventIsHandled()
    {
        ContractDomainEvents.ContractCreatedEvent contractCreatedEvent = TestData.DomainEvents.ContractCreatedEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(contractCreatedEvent, TestContext.Current.CancellationToken); });
    }

    [Fact]
    public void ContractDomainEventHandler_FixedValueProductAddedToContractEvent_EventIsHandled()
    {
        ContractDomainEvents.FixedValueProductAddedToContractEvent fixedValueProductAddedToContractEvent = TestData.DomainEvents.FixedValueProductAddedToContractEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(fixedValueProductAddedToContractEvent, TestContext.Current.CancellationToken); });
    }

    [Fact]
    public void ContractDomainEventHandler_TransactionFeeForProductAddedToContractEvent_EventIsHandled()
    {
        ContractDomainEvents.TransactionFeeForProductAddedToContractEvent transactionFeeForProductAddedToContractEvent = TestData.DomainEvents.TransactionFeeForProductAddedToContractEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(transactionFeeForProductAddedToContractEvent, TestContext.Current.CancellationToken); });
    }

    [Fact]
    public void ContractDomainEventHandler_TransactionFeeForProductDisabledEvent_EventIsHandled()
    {
        ContractDomainEvents.TransactionFeeForProductDisabledEvent transactionFeeForProductDisabledEvent = TestData.DomainEvents.TransactionFeeForProductDisabledEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(transactionFeeForProductDisabledEvent, TestContext.Current.CancellationToken); });
    }

    [Fact]
    public void ContractDomainEventHandler_VariableValueProductAddedToContractEvent_EventIsHandled()
    {
        ContractDomainEvents.VariableValueProductAddedToContractEvent variableValueProductAddedToContractEvent = TestData.DomainEvents.VariableValueProductAddedToContractEvent;

        Should.NotThrow(async () => { await this.DomainEventHandler.Handle(variableValueProductAddedToContractEvent, TestContext.Current.CancellationToken); });
    }

    #endregion
}
