using Imposter.Abstractions;
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

public class MerchantScheduleDomainEventHandlerTests
{
    private readonly ITransactionProcessorReadModelRepositoryImposter EstateReportingRepository;
    private readonly ReadModelDomainEventHandler DomainEventHandler;

    public MerchantScheduleDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        this.EstateReportingRepository = new ITransactionProcessorReadModelRepositoryImposter();
        this.DomainEventHandler = new ReadModelDomainEventHandler(this.EstateReportingRepository.Instance());
    }

    [Fact]
    public async Task MerchantScheduleDomainEventHandler_MerchantScheduleCreatedEvent_EventIsHandled()
    {
        MerchantScheduleDomainEvents.MerchantScheduleCreatedEvent domainEvent = TestData.DomainEvents.MerchantScheduleCreatedEvent;
        this.EstateReportingRepository.AddMerchantSchedule(domainEvent, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        this.EstateReportingRepository.AddMerchantSchedule(domainEvent, Arg<CancellationToken>.Any()).Called(Count.Once());
    }

    [Fact]
    public async Task MerchantScheduleDomainEventHandler_MerchantScheduleMonthUpdatedEvent_EventIsHandled()
    {
        MerchantScheduleDomainEvents.MerchantScheduleMonthUpdatedEvent domainEvent = TestData.DomainEvents.MerchantScheduleMonthUpdatedEvent;
        this.EstateReportingRepository.UpdateMerchantSchedule(domainEvent, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.DomainEventHandler.Handle(domainEvent, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        this.EstateReportingRepository.UpdateMerchantSchedule(domainEvent, Arg<CancellationToken>.Any()).Called(Count.Once());
    }
}

