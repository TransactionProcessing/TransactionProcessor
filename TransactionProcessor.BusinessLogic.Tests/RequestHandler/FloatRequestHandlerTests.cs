using System;
using System.Threading.Tasks;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Tests.RequestHandler;

using System.Threading;
using BusinessLogic.Services;
using Moq;
using RequestHandlers;
using Requests;
using Shouldly;
using Testing;
using Xunit;

public class FloatRequestHandlerTests
{
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