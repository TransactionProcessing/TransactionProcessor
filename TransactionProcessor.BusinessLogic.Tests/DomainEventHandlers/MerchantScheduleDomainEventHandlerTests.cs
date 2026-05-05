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

public class MerchantScheduleDomainEventHandlerTests
{
    private readonly Mock<ITransactionProcessorReadModelRepository> EstateReportingRepository;
    private readonly ReadModelDomainEventHandler DomainEventHandler;

    public MerchantScheduleDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
        this.EstateReportingRepository = new Mock<ITransactionProcessorReadModelRepository>();
        this.DomainEventHandler = new ReadModelDomainEventHandler(this.EstateReportingRepository.Object);
    }

    [Fact]
    public async Task MerchantScheduleDomainEventHandler_MerchantScheduleCreatedEvent_EventIsHandled()
    {
        MerchantScheduleDomainEvents.MerchantScheduleCreatedEvent domainEvent = TestData.DomainEvents.MerchantScheduleCreatedEvent;
        this.EstateReportingRepository
            .Setup(r => r.AddMerchantSchedule(domainEvent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        Result result = await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        this.EstateReportingRepository.Verify(r => r.AddMerchantSchedule(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MerchantScheduleDomainEventHandler_MerchantScheduleMonthUpdatedEvent_EventIsHandled()
    {
        MerchantScheduleDomainEvents.MerchantScheduleMonthUpdatedEvent domainEvent = TestData.DomainEvents.MerchantScheduleMonthUpdatedEvent;
        this.EstateReportingRepository
            .Setup(r => r.UpdateMerchantSchedule(domainEvent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        Result result = await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        this.EstateReportingRepository.Verify(r => r.UpdateMerchantSchedule(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
    }
}
