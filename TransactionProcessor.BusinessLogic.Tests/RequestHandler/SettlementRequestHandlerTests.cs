using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.RequestHandlers;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler;

public class SettlementRequestHandlerTests
{
    [Fact]
    public async Task SettlementRequestHandler_ProcessSettlementRequest_IsHandled()
    {
        Mock<ISettlementDomainService> settlementDomainService = new Mock<ISettlementDomainService>();
        Mock<IAggregateRepository<SettlementAggregate, DomainEvent>> settlementAggregateRepository = new();
        SettlementRequestHandler handler = new SettlementRequestHandler(settlementDomainService.Object, settlementAggregateRepository.Object);
        settlementDomainService
            .Setup(s => s.ProcessSettlement(It.IsAny<SettlementCommands.ProcessSettlementCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var command = TestData.ProcessSettlementCommand;

        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
}
