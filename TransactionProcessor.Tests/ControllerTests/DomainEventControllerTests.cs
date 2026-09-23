using Imposter.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventHandling;
using Shared.General;
using Shared.Serialisation;
using Shared.Logger;
using TransactionProcessor.Controllers;
using TransactionProcessor.DomainEvents;
using Xunit;

namespace TransactionProcessor.Tests.ControllerTests;

public class DomainEventControllerTests
{
    [Fact]
    public async Task PostEventAsync_WhenAllHandlersSucceed_ReturnsOk()
    {
        InitialiseDependencies();
        DomainEvent domainEvent = CreateDomainEvent();
        DomainEventController controller = CreateController(
            domainEvent,
            new ResultDomainEventHandler(Result.Success()));

        IResult result = await controller.PostEventAsync(domainEvent, CancellationToken.None);
        
        result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task PostEventAsync_WhenNoHandlersAreConfigured_ReturnsOk()
    {
        InitialiseDependencies();
        DomainEvent domainEvent = CreateDomainEvent();
        DomainEventController controller = CreateController(domainEvent);

        IResult result = await controller.PostEventAsync(domainEvent, CancellationToken.None);

        result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task PostEventAsync_WhenHandlerReturnsFailure_ReturnsDetailedProblemDetails()
    {
        InitialiseDependencies();
        DomainEvent domainEvent = CreateDomainEvent();
        IDomainEventHandler handler = new ResultDomainEventHandler(Result.Failure("Transaction was not found"));

        DomainEventController controller = CreateController(domainEvent, handler);

        ObjectResult result = (ObjectResult)await controller.PostEventAsync(domainEvent, CancellationToken.None);

        result.StatusCode.ShouldBe(500);
        ProblemDetails problemDetails = (ProblemDetails)result.Value;
        problemDetails.Title.ShouldBe("One or more event handlers failed");
        problemDetails.Status.ShouldBe(500);
        problemDetails.Extensions["eventId"].ShouldBeOfType<Guid>();
        problemDetails.Extensions["eventType"].ShouldBe(domainEvent.GetType().Name);
        String failuresJson = System.Text.Json.JsonSerializer.Serialize(problemDetails.Extensions["failures"]);
        failuresJson.ShouldContain("Transaction was not found");
        failuresJson.ShouldContain(nameof(ResultDomainEventHandler));
    }

    [Fact]
    public async Task PostEventAsync_WhenMultipleHandlersFail_ReturnsEveryFailure()
    {
        InitialiseDependencies();
        DomainEvent domainEvent = CreateDomainEvent();

        DomainEventController controller = CreateController(
            domainEvent,
            new ResultDomainEventHandler(Result.Failure("First failure")),
            new ThrowingDomainEventHandler("Second failure"));

        ObjectResult result = (ObjectResult)await controller.PostEventAsync(domainEvent, CancellationToken.None);

        String failuresJson = System.Text.Json.JsonSerializer.Serialize(((ProblemDetails)result.Value).Extensions["failures"]);
        result.StatusCode.ShouldBe(500);
        failuresJson.ShouldContain("First failure");
        failuresJson.ShouldContain("Second failure");
        failuresJson.ShouldNotContain("StackTrace");
    }

    private static DomainEventController CreateController(DomainEvent domainEvent, params IDomainEventHandler[] handlers)
    {
        IDomainEventHandlerResolverImposter resolver = new();
        resolver.GetDomainEventHandlers(Arg<IDomainEvent>.Any())
            .Returns(Result.Success(handlers.ToList()));

        DomainEventController controller = new(resolver.Instance())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = new ServiceCollection()
                        .AddMvcCore()
                        .Services
                        .BuildServiceProvider()
                }
            }
        };
        controller.Request.Headers["eventType"] = domainEvent.GetType().Name;
        return controller;
    }

    private static DomainEvent CreateDomainEvent()
    {
        DomainEvent domainEvent = new TransactionDomainEvents.ProductDetailsAddedToTransactionEvent(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        TypeMap.AddType(domainEvent.GetType(), domainEvent.GetType().Name);
        return domainEvent;
    }

    private static void InitialiseDependencies()
    {
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new System.Text.Json.JsonSerializerOptions()));
        Logger.Initialise(NullLogger.Instance);
    }

    private sealed class ResultDomainEventHandler : IDomainEventHandler
    {
        private readonly Result Result;

        public ResultDomainEventHandler(Result result) => this.Result = result;

        public Task<Result> Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
            => Task.FromResult(this.Result);
    }

    private sealed class ThrowingDomainEventHandler : IDomainEventHandler
    {
        private readonly String Error;

        public ThrowingDomainEventHandler(String error) => this.Error = error;

        public Task<Result> Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
            => throw new InvalidOperationException(this.Error);
    }
}
