using System;
using System.Threading;
using System.Threading.Tasks;
using Lamar;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SimpleResults;
using TransactionProcessor.BusinessLogic.Requests;
using TransactionProcessor.BusinessLogic.Validation;
using TransactionProcessor.Bootstrapper;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using Xunit;

namespace TransactionProcessor.BusinessLogic.Tests.Validation;

public sealed class CreateEstateCommandValidationBehaviorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_InvalidEstateName_ReturnsInvalidWithoutCallingHandler(String estateName)
    {
        ValidationBehavior<EstateCommands.CreateEstateCommand, Guid> behavior = new(new CreateEstateCommandValidator());
        EstateCommands.CreateEstateCommand command = new(new CreateEstateRequest { EstateName = estateName });
        Boolean handlerWasCalled = false;
        RequestHandlerDelegate<Result<Guid>> next = _ =>
        {
            handlerWasCalled = true;
            return Task.FromResult(Result.Success(Guid.NewGuid()));
        };

        Result<Guid> result = await behavior.Handle(command, next, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        handlerWasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ValidEstateName_CallsHandler()
    {
        ValidationBehavior<EstateCommands.CreateEstateCommand, Guid> behavior = new(new CreateEstateCommandValidator());
        EstateCommands.CreateEstateCommand command = new(new CreateEstateRequest { EstateName = "Example Estate" });
        Guid expectedEstateId = Guid.NewGuid();
        RequestHandlerDelegate<Result<Guid>> next = _ => Task.FromResult(Result.Success(expectedEstateId));

        Result<Guid> result = await behavior.Handle(command, next, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBe(expectedEstateId);
    }

    [Fact]
    public async Task Handle_NullRequestDto_ReturnsInvalidWithoutCallingHandler()
    {
        ValidationBehavior<EstateCommands.CreateEstateCommand, Guid> behavior = new(new CreateEstateCommandValidator());
        EstateCommands.CreateEstateCommand command = new(null);
        Boolean handlerWasCalled = false;
        RequestHandlerDelegate<Result<Guid>> next = _ =>
        {
            handlerWasCalled = true;
            return Task.FromResult(Result.Success(Guid.NewGuid()));
        };

        Result<Guid> result = await behavior.Handle(command, next, CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        handlerWasCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task MediatorSend_InvalidEstateName_ReturnsInvalidBeforeHandler()
    {
        MediatorRegistry registry = new();
        registry.AddLogging();
        using Container container = new(registry);
        IMediator mediator = container.GetInstance<IMediator>();

        Result<Guid> result = await mediator.Send(
            new EstateCommands.CreateEstateCommand(new CreateEstateRequest { EstateName = "   " }),
            TestContext.Current.CancellationToken);

        result.Status.ShouldBe(ResultStatus.Invalid);
    }
}
