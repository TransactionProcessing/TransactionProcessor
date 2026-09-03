using Imposter.Abstractions;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.Logger;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class ReadModelDomainEventHandlerTests
{
    private sealed record UnhandledDomainEvent() : DomainEvent(Guid.NewGuid(), Guid.NewGuid());

    private readonly ITransactionProcessorReadModelRepositoryImposter EstateReportingRepository;
    private readonly ReadModelDomainEventHandler DomainEventHandler;

    public ReadModelDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        this.EstateReportingRepository = new ITransactionProcessorReadModelRepositoryImposter();
        this.DomainEventHandler = new ReadModelDomainEventHandler(this.EstateReportingRepository.Instance());
    }

    [Fact]
    public async Task ReadModelDomainEventHandler_Handle_AdditionalRequestDataRecordedEvent_RecordSucceeds_SetsTransactionAmount()
    {
        TransactionDomainEvents.AdditionalRequestDataRecordedEvent domainEvent = new(TestData.TransactionId,
                                                                                      TestData.EstateId,
                                                                                      TestData.MerchantId,
                                                                                      TestData.OperatorId,
                                                                                      new Dictionary<string, string>(),
                                                                                      TestData.TransactionDateTime);

        this.EstateReportingRepository.RecordTransactionAdditionalRequestData(domainEvent, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());
        this.EstateReportingRepository.SetTransactionAmount(domainEvent, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        this.EstateReportingRepository.RecordTransactionAdditionalRequestData(domainEvent, Arg<CancellationToken>.Any()).Called(Count.Once());
        this.EstateReportingRepository.SetTransactionAmount(domainEvent, Arg<CancellationToken>.Any()).Called(Count.Once());
    }

    [Fact]
    public async Task ReadModelDomainEventHandler_Handle_StatementGeneratedEvent_MarkFails_DoesNotUpdateMerchant()
    {
        MerchantStatementDomainEvents.StatementGeneratedEvent domainEvent = TestData.DomainEvents.StatementGeneratedEvent;

        this.EstateReportingRepository.MarkStatementAsGenerated(domainEvent, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        Result result = await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken);

        result.IsFailed.ShouldBeTrue();
        this.EstateReportingRepository.MarkStatementAsGenerated(domainEvent, Arg<CancellationToken>.Any()).Called(Count.Once());
        this.EstateReportingRepository.UpdateMerchant(domainEvent, Arg<CancellationToken>.Any()).Called(Count.Never());
    }

    [Fact]
    public async Task ReadModelDomainEventHandler_Handle_UnhandledDomainEvent_ReturnsSuccessWithoutRepositoryCalls()
    {
        UnhandledDomainEvent domainEvent = new();

        Result result = await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
    }
}

