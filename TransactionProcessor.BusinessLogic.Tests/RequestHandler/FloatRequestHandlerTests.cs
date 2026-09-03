using SimpleResults;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler;

using BusinessLogic.Services;
using Imposter.Abstractions;
using RequestHandlers;
using Requests;
using Shared.Serialisation;
using Shouldly;
using System.Text.Json;
using System.Threading;
using Testing;
using Xunit;

public class FloatRequestHandlerTests
{
    public FloatRequestHandlerTests() {
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
    }

    [Fact]
    public async Task FloatRequestHandler_CreateFloatForContractProductRequest_IsHandled(){
        IFloatDomainServiceImposter floatDomainService = new();
        FloatRequestHandler handler = new FloatRequestHandler(floatDomainService.Instance());
        floatDomainService.CreateFloat(Arg<FloatCommands.CreateFloatCommand>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());
        var command = TestData.CreateFloatCommand;

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task FloatRequestHandler_RecordCreditPurchaseForFloatRequest_IsHandled()
    {
        IFloatDomainServiceImposter floatDomainService = new();
        FloatRequestHandler handler = new FloatRequestHandler(floatDomainService.Instance());
        
        floatDomainService.RecordCreditPurchase(Arg<FloatCommands.RecordCreditPurchaseForFloatCommand>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        var command = TestData.RecordCreditPurchaseForFloatCommand;

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();

    }
}
