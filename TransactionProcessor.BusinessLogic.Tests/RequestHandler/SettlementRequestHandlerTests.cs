using Imposter.Abstractions;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Serialisation;
using Shouldly;
using SimpleResults;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Manager;
using TransactionProcessor.BusinessLogic.RequestHandlers;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler;

public class SettlementRequestHandlerTests
{
    public SettlementRequestHandlerTests() {
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
    }

    [Fact]
    public async Task SettlementRequestHandler_ProcessSettlementRequest_IsHandled()
    {
        ISettlementDomainServiceImposter settlementDomainService = new();
        ITransactionProcessorManagerImposter manager = new();
        SettlementRequestHandler handler = new SettlementRequestHandler(settlementDomainService.Instance(), manager.Instance());
        settlementDomainService
            .ProcessSettlement(Arg<SettlementCommands.ProcessSettlementCommand>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        var command = TestData.Commands.ProcessSettlementCommand;

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }
}

