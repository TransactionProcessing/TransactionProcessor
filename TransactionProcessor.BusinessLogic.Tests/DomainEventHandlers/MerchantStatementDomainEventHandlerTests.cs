using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Grpc.Core;
using MediatR;
using Moq;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.Testing;
using Xunit;
using Xunit.Abstractions;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class MerchantStatementDomainEventHandlerTests : DomainEventHandlerTests
{
    private readonly MerchantStatementDomainEventHandler EventHandler;
    public MerchantStatementDomainEventHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        this.EventHandler = new MerchantStatementDomainEventHandler(this.Mediator.Object);
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementGeneratedEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementGeneratedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementGeneratedEvent_Retry_EventIsHandled()
    {
        this.Mediator.SetupSequence(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new List<String>() { "Append failed due to WrongExpectedVersion" }))
            .ReturnsAsync(Result.Failure(new List<String>() { "DeadlineExceeded" }))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementGeneratedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.TransactionHasBeenCompletedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_Retry_EventIsHandled()
    {
        this.Mediator.SetupSequence(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new List<String>() { "Append failed due to WrongExpectedVersion" }))
            .ReturnsAsync(Result.Failure(new List<String>() { "DeadlineExceeded" }))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.TransactionHasBeenCompletedEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
}