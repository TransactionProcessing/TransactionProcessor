using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Repository;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class ReadModelDomainEventHandlerTests
{
    private readonly Mock<ITransactionProcessorReadModelRepository> EstateReportingRepository;
    private readonly ReadModelDomainEventHandler DomainEventHandler;

    public ReadModelDomainEventHandlerTests()
    {
        Logger.Initialise(NullLogger.Instance);
        this.EstateReportingRepository = new Mock<ITransactionProcessorReadModelRepository>();
        this.DomainEventHandler = new ReadModelDomainEventHandler(this.EstateReportingRepository.Object);
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

        this.EstateReportingRepository
            .Setup(r => r.RecordTransactionAdditionalRequestData(domainEvent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.EstateReportingRepository
            .Setup(r => r.SetTransactionAmount(domainEvent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        Result result = await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        this.EstateReportingRepository.Verify(r => r.RecordTransactionAdditionalRequestData(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
        this.EstateReportingRepository.Verify(r => r.SetTransactionAmount(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReadModelDomainEventHandler_Handle_StatementGeneratedEvent_MarkFails_DoesNotUpdateMerchant()
    {
        MerchantStatementDomainEvents.StatementGeneratedEvent domainEvent = TestData.DomainEvents.StatementGeneratedEvent;

        this.EstateReportingRepository
            .Setup(r => r.MarkStatementAsGenerated(domainEvent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure);

        Result result = await this.DomainEventHandler.Handle(domainEvent, CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        this.EstateReportingRepository.Verify(r => r.MarkStatementAsGenerated(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
        this.EstateReportingRepository.Verify(r => r.UpdateMerchant(domainEvent, It.IsAny<CancellationToken>()), Times.Never);
    }
}
