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
    public async Task MerchantStatementDomainEventHandler_Handle_StatementBuiltEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementBuiltEvent, CancellationToken.None);
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
    public async Task MerchantStatementDomainEventHandler_Handle_MerchantFeeSettledEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.MerchantFeeSettledEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementCreatedForDateEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementCreatedForDateEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_AutomaticDepositMadeEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.AutomaticDepositMadeEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_ManualDepositMadeEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.ManualDepositMadeEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_WithdrawalMadeEvent_EventIsHandled()
    {
        this.Mediator.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.WithdrawalMadeEvent, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
}