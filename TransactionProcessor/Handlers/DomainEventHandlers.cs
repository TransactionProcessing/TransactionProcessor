using Results = TransactionProcessing.UnionResults.Results;
using Lamar;
using Microsoft.AspNetCore.Http;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.Shared.EventStore.EventHandling;
using TransactionProcessing.Shared.Exceptions;
using TransactionProcessing.Shared.General;
using TransactionProcessing.Shared.Logger;
using TransactionProcessing.Shared.Serialisation;
using TransactionProcessing.UnionResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionProcessor.Handlers;

public static class DomainEventHandlers {
    public static async Task<Result> HandleDomainEvent(IDomainEventHandlerResolver domainEventHandlerResolver, IContainer container, HttpContext ctx, CancellationToken cancellationToken) {

        String eventType = ctx.Request.Headers["eventType"].ToString();
        String subscriptionId = ctx.Request.Headers["subscriptionId"].ToString();
        if (String.IsNullOrWhiteSpace(subscriptionId))
            subscriptionId = null;

        // TODO: Reject empty event type

        Result<IDomainEvent> domainEventResult = await GetDomainEvent(ctx.Request, eventType, cancellationToken);

        if (domainEventResult.IsSuccess() == false)
            return domainEventResult.Bind(_ => Results.Ok());

        IDomainEvent domainEvent = domainEventResult.Data;

        List<IDomainEventHandler> eventHandlers = GetDomainEventHandlers(container, domainEventHandlerResolver, domainEvent, subscriptionId);

        cancellationToken.Register(() => Callback(cancellationToken, domainEvent.EventId));

        try {
            Logger.LogWarning($"Processing event - ID [{domainEvent.EventId}], Type[{domainEvent.GetType().Name}]");

            if (eventHandlers == null || eventHandlers.Any() == false) {
                // Log a warning out 
                Logger.LogInformation($"No event handlers configured for Event Type [{domainEvent.GetType().Name}]");
                return Results.Ok();
            }

            List<HandlerExecution> executions = eventHandlers
                .Select(domainEventHandler => ExecuteHandler(domainEventHandler, domainEvent, cancellationToken))
                .ToList();

            try {
                await Task.WhenAll(executions.Select(execution => execution.Task));
            }
            catch (Exception ex) {
                Logger.LogError(ex, "One or more handlers threw while processing event [{EventId}]", domainEvent.EventId);
            }

            List<HandlerFailure> failures = executions.SelectMany(GetFailures).ToList();
            if (failures.Any()) {
                String failureDetails = String.Join("; ", failures.Select(failure => $"{failure.Handler}: {failure.Error}"));
                return Results.Fail(new Error(ErrorType.Critical, $"One or more event handlers failed for event {domainEvent.EventId} ({domainEvent.GetType().Name}): {failureDetails}"));
            }

            Logger.LogWarning($"Finished processing event - ID [{domainEvent.EventId}]");

            return Results.Ok();
        }
        catch (Exception ex) {
            String domainEventData = StringSerialiser.Serialise(domainEvent);
            Logger.LogError(ex, " Failed to Process Event Id {EventId}, Event Data received [" + domainEventData + "]", domainEvent.EventId);

            throw;
        }
    }

    private static IEnumerable<HandlerFailure> GetFailures(HandlerExecution execution) {
        if (execution.Task.IsFaulted) {
            String error = String.Join("; ", execution.Task.Exception?.Flatten().InnerExceptions
                                                      .SelectMany(exception => exception.GetExceptionMessages()) ?? Enumerable.Empty<String>());
            yield return new HandlerFailure(execution.Handler.GetType().Name, error);
        }
        else if (execution.Task.IsCanceled) {
            yield return new HandlerFailure(execution.Handler.GetType().Name, "Handler execution was cancelled");
        }
        else if (execution.Task.Result.IsSuccess() == false) {
            yield return new HandlerFailure(execution.Handler.GetType().Name, execution.Task.Result.GetErrorMessage());
        }
    }
    private sealed record HandlerFailure(String Handler, String Error);
    private sealed record HandlerExecution(IDomainEventHandler Handler, Task<Result> Task);

    private static void Callback(CancellationToken cancellationToken,
                          Guid eventId) {
        if (cancellationToken.IsCancellationRequested) //I think this would always be true anyway
        {
            Logger.LogInformation($"Cancel request for EventId {eventId}");
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private static HandlerExecution ExecuteHandler(IDomainEventHandler handler,
                                            IDomainEvent domainEvent,
                                            CancellationToken cancellationToken) {
        try {
            return new HandlerExecution(handler, handler.Handle(domainEvent, cancellationToken));
        }
        catch (Exception ex) {
            return new HandlerExecution(handler, Task.FromException<Result>(ex));
        }
    }

    private static async Task<Result<IDomainEvent>> GetDomainEvent(HttpRequest request, String eventType, CancellationToken cancellationToken) {
        Type type;

        try {
            type = TypeMap.GetType(eventType);
        }
        catch (Exception ex) {
            // Just swallowing exception here...
            type = null;
        }

        if (type == null)
            return new Error(ErrorType.NotFound, $"Failed to find a domain event with type {eventType}");

        if (type.IsSubclassOf(typeof(DomainEvent))) {
            using JsonDocument requestBody = await JsonDocument.ParseAsync(request.Body, cancellationToken: cancellationToken);
            String json = requestBody.RootElement.GetRawText();

            DomainEventFactory domainEventFactory = new();
            return Results.Ok(domainEventFactory.CreateDomainEvent(json, type) as IDomainEvent);
        }

        return new Error(ErrorType.Validation, $"The requested type {type.FullName} is not a domain event type.");
    }

    private static List<IDomainEventHandler> GetDomainEventHandlers(IContainer container, IDomainEventHandlerResolver domainEventHandlerResolver, IDomainEvent domainEvent, String subscriptionId) {

        if (subscriptionId != null) {
            var resolver = container.GetInstance<IDomainEventHandlerResolver>(subscriptionId);
            // We are being told by the caller to use a specific handler
            var allhandlers = resolver.GetDomainEventHandlers(domainEvent);

            if (allhandlers.IsSuccess() == false)
                return new List<IDomainEventHandler>();

            return allhandlers.Data.ToList();
        }

        var eventHandlersResult = domainEventHandlerResolver.GetDomainEventHandlers(domainEvent);
        if (eventHandlersResult.IsSuccess() == false)
            return new List<IDomainEventHandler>();
        return eventHandlersResult.Data;
    }

}
