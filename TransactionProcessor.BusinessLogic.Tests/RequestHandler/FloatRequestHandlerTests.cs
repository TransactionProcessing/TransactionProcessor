using SimpleResults;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler;

using BusinessLogic.Services;
using Moq;
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
        Mock<IFloatDomainService> floatDomainService = new Mock<IFloatDomainService>();
        FloatRequestHandler handler = new FloatRequestHandler(floatDomainService.Object);
        floatDomainService.Setup(f => f.CreateFloatForContractProduct(It.IsAny<FloatCommands.CreateFloatForContractProductCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());
        var command = TestData.CreateFloatForContractProductCommand;

        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task FloatRequestHandler_RecordCreditPurchaseForFloatRequest_IsHandled()
    {
        Mock<IFloatDomainService> floatDomainService = new Mock<IFloatDomainService>();
        FloatRequestHandler handler = new FloatRequestHandler(floatDomainService.Object);
        
        floatDomainService.Setup(f => f.RecordCreditPurchase(It.IsAny<FloatCommands.RecordCreditPurchaseForFloatCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        var command = TestData.RecordCreditPurchaseForFloatCommand;

        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

    }
}