namespace TransactionProcessor.BusinessLogic.Tests.CommandHandler;

using System.Threading;
using BusinessLogic.Services;
using Moq;
using RequestHandlers;
using Requests;
using Shouldly;
using Testing;
using Xunit;

public class SettlementRequestHandlerTests
{
    [Fact]
    public void SettlementRequestHandler_ProcessSettlementRequest_IsHandled()
    {
        Mock<ISettlementDomainService> settlementDomainService = new Mock<ISettlementDomainService>();
        SettlementRequestHandler handler = new SettlementRequestHandler(settlementDomainService.Object);

        ProcessSettlementRequest command = TestData.ProcessSettlementRequest;

        Should.NotThrow(async () =>
                        {
                            await handler.Handle(command, CancellationToken.None);
                        });

    }
}