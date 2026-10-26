using Imposter.Abstractions;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventHandling;
using Shared.General;
using Shared.Logger;
using Shared.Serialisation;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Endpoints;
using Xunit;

namespace TransactionProcessor.Tests.ControllerTests;

public class DomainEventEndpointTests
{
    [Fact]
    public async Task PostEvent_WhenAllHandlersSucceed_ReturnsOk()
    {
        DomainEvent domainEvent = CreateDomainEvent();
        await using WebApplication app = await CreateApplication(new ResultDomainEventHandler(Result.Success()));
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await PostDomainEvent(client, domainEvent);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostEvent_WhenNoHandlersAreConfigured_ReturnsOk()
    {
        DomainEvent domainEvent = CreateDomainEvent();
        await using WebApplication app = await CreateApplication();
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await PostDomainEvent(client, domainEvent);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostEvent_WhenEventTypeIsUnknown_ReturnsNotFound()
    {
        DomainEvent domainEvent = CreateDomainEvent();
        await using WebApplication app = await CreateApplication();
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await PostDomainEvent(client, domainEvent, "UnknownDomainEvent");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostEvent_WhenHandlerReturnsFailure_ReturnsDetailedProblemDetails()
    {
        DomainEvent domainEvent = CreateDomainEvent();
        await using WebApplication app = await CreateApplication(new ResultDomainEventHandler(Result.Failure("Transaction was not found")));
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await PostDomainEvent(client, domainEvent);
        String responseBody = await response.Content.ReadAsStringAsync();
        using JsonDocument problem = JsonDocument.Parse(responseBody);
        JsonElement root = problem.RootElement;

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        root.GetProperty("title").GetString().ShouldBe("One or more event handlers failed");
        root.GetProperty("status").GetInt32().ShouldBe(500);
        root.GetProperty("eventId").GetGuid().ShouldBe(domainEvent.EventId);
        root.GetProperty("eventType").GetString().ShouldBe(domainEvent.GetType().Name);
        String failuresJson = root.GetProperty("failures").GetRawText();
        failuresJson.ShouldContain("Transaction was not found");
        failuresJson.ShouldContain(nameof(ResultDomainEventHandler));
    }

    [Fact]
    public async Task PostEvent_WhenMultipleHandlersFail_ReturnsEveryFailure()
    {
        DomainEvent domainEvent = CreateDomainEvent();
        await using WebApplication app = await CreateApplication(
            new ResultDomainEventHandler(Result.Failure("First failure")),
            new ThrowingDomainEventHandler("Second failure"));
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await PostDomainEvent(client, domainEvent);
        String responseBody = await response.Content.ReadAsStringAsync();
        using JsonDocument problem = JsonDocument.Parse(responseBody);
        String failuresJson = problem.RootElement.GetProperty("failures").GetRawText();

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        failuresJson.ShouldContain("First failure");
        failuresJson.ShouldContain("Second failure");
        failuresJson.ShouldNotContain("StackTrace");
    }

    private static async Task<WebApplication> CreateApplication(params IDomainEventHandler[] handlers)
    {
        InitialiseDependencies();
        IDomainEventHandlerResolverImposter resolver = new();
        resolver.GetDomainEventHandlers(Arg<IDomainEvent>.Any())
            .Returns(Result.Success(handlers.ToList()));

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IDomainEventHandlerResolver>(resolver.Instance());
        builder.Services.AddSingleton<IContainer>(new Container(new ServiceRegistry()));

        WebApplication app = builder.Build();
        app.MapDomainEventEndpoints();
        await app.StartAsync();
        return app;
    }

    private static async Task<HttpResponseMessage> PostDomainEvent(HttpClient client, DomainEvent domainEvent, String eventType = null)
    {
        String json = StringSerialiser.Serialise(domainEvent);
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/domainevents")
        {
            Content = content
        };
        request.Headers.Add("eventType", eventType ?? domainEvent.GetType().Name);

        return await client.SendAsync(request, CancellationToken.None);
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
        StringSerialiser.Initialise(new SystemTextJsonSerializer(new JsonSerializerOptions()));
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
