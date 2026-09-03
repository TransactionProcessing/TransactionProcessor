using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Grpc.Core;
using Imposter.Abstractions;
using MediatR;
using Shared.Logger;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.EventHandling;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.DomainEventHandlers;

public class MerchantStatementDomainEventHandlerTests : DomainEventHandlerTests
{
    private readonly MerchantStatementDomainEventHandler EventHandler;
    public MerchantStatementDomainEventHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        this.EventHandler = new MerchantStatementDomainEventHandler(this.Mediator.Instance());
        Logger.Initialise(new NullLogger());
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementGeneratedEvent_EventIsHandled()
    {
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementGeneratedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementBuiltEvent_EventIsHandled()
    {
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementBuiltEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_EventIsHandled()
    {
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.TransactionHasBeenCompletedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_WrongExpectedRetried_EventIsHandled()
    {
        List<String> errors = new() { "WrongExpectedVersion" };

        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure(errors))
            .Then()
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.TransactionHasBeenCompletedEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(2));
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_TransactionHasBeenCompletedEvent_WrongExpectedRetried_AllRetriesFailed()
    {
        List<String> errors = new() { "WrongExpectedVersion" };
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors));

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.TransactionHasBeenCompletedEvent, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(6));
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_MerchantFeeSettledEvent_EventIsHandled()
    {
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.MerchantFeeSettledEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_MerchantFeeSettledEvent_WrongExpectedRetried_EventIsHandled()
    {
        List<String> errors = new() { "WrongExpectedVersion" };

        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure(errors))
            .Then()
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.MerchantFeeSettledEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(2));
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_MerchantFeeSettledEvent_WrongExpectedRetried_AllRetriesFailed()
    {
        List<String> errors = new() { "WrongExpectedVersion" };
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors))
            .Then().ReturnsAsync(Result.Failure(errors));

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.MerchantFeeSettledEvent, TestContext.Current.CancellationToken);
        result.IsFailed.ShouldBeTrue();
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(6));
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_StatementCreatedForDateEvent_EventIsHandled()
    {
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.StatementCreatedForDateEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_AutomaticDepositMadeEvent_EventIsHandled()
    {
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.AutomaticDepositMadeEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_ManualDepositMadeEvent_EventIsHandled()
    {
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.ManualDepositMadeEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task MerchantStatementDomainEventHandler_Handle_WithdrawalMadeEvent_EventIsHandled()
    {
        this.Mediator.Send<Result>(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        Result result = await this.EventHandler.Handle(TestData.DomainEvents.WithdrawalMadeEvent, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
    }
}
