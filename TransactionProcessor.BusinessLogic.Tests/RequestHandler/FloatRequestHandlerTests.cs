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
    public void FloatRequestHandler_CreateFloatForContractProductRequest_IsHandled(){
        Mock<IFloatDomainService> floatDomainService = new Mock<IFloatDomainService>();
        FloatRequestHandler handler = new FloatRequestHandler(floatDomainService.Object);

        CreateFloatForContractProductRequest command = TestData.CreateFloatForContractProductRequest;

        Should.NotThrow(async () => { await handler.Handle(command, CancellationToken.None); });
    }

    [Fact]
    public void FloatRequestHandler_RecordCreditPurchaseForFloatRequest_IsHandled()
    {
        Mock<IFloatDomainService> floatDomainService = new Mock<IFloatDomainService>();
        FloatRequestHandler handler = new FloatRequestHandler(floatDomainService.Object);

        RecordCreditPurchaseForFloatRequest command = TestData.RecordCreditPurchaseForFloatRequest;

        Should.NotThrow(async () =>
                        {
                            await handler.Handle(command, CancellationToken.None);
                        });

    }
}