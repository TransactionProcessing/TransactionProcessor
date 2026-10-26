using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Exceptions;
using Shared.Results;
using Shared.Results.Web;

namespace TransactionProcessor.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Lamar;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.General;
    using Shared.Logger;
    using Shared.Serialisation;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using TransactionProcessor.BusinessLogic.Services;

    [Route(DomainEventController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class DomainEventController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// The domain event handler resolver
        /// </summary>
        private readonly IDomainEventHandlerResolver DomainEventHandlerResolver;
        private readonly IContainer Container;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventController"/> class.
        /// </summary>
        /// <param name="domainEventHandlerResolver">The domain event handler resolver.</param>
        /// <param name="container">The application service container.</param>
        public DomainEventController(IDomainEventHandlerResolver domainEventHandlerResolver, IContainer container)
        {
            this.DomainEventHandlerResolver = domainEventHandlerResolver;
            this.Container = container;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Posts the event asynchronous.
        /// </summary>
        /// <param name="request" example="{&quot;merchantId&quot;:&quot;00000000-0000-0000-0000-000000000227&quot;,&quot;estateId&quot;:&quot;00000000-0000-0000-0000-000000000023&quot;,&quot;merchantName&quot;:&quot;Example Merchant&quot;,&quot;dateCreated&quot;:&quot;2026-01-15T10:30:00Z&quot;}">The event payload. This example is for MerchantCreatedEvent; event fields vary with eventType.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="eventType">Required eventType header naming the domain event type.</param>
        /// <param name="subscriptionId">Optional subscriptionId header selecting a named event handler resolver.</param>
        /// <returns></returns>
        /// <remarks>
        /// Set eventType to <c>MerchantCreatedEvent</c> when using the body example. Other event types require their own payload shape.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IResult> PostEventAsync([FromBody] Object request,
                                                        CancellationToken cancellationToken,
                                                        [FromHeader(Name = "eventType")] String eventType,
                                                        [FromHeader(Name = "subscriptionId")] String subscriptionId = null)
        {
            Result<IDomainEvent> domainEventResult = await this.GetDomainEvent(request, eventType);

            if (domainEventResult.IsFailed)
                return ResponseFactory.FromResult(domainEventResult, SuccessFactory);
            
            IDomainEvent domainEvent = domainEventResult.Data;

            List<IDomainEventHandler> eventHandlers = this.GetDomainEventHandlers(domainEvent, subscriptionId);

            cancellationToken.Register(() => this.Callback(cancellationToken, domainEvent.EventId));

            try
            {
                Logger.LogWarning("Processing event - ID [{EventId}], Type[{EventType}]", domainEvent.EventId, domainEvent.GetType().Name);

                if (eventHandlers == null || eventHandlers.Any() == false) {
                    // Log a warning out 
                    Logger.LogInformation("No event handlers configured for Event Type [{EventType}]", domainEvent.GetType().Name);
                    return Results.Ok();
                }

                List<HandlerExecution> executions = eventHandlers
                                                    .Select(domainEventHandler => this.ExecuteHandler(domainEventHandler, domainEvent, cancellationToken))
                                                    .ToList();

                try {
                    await Task.WhenAll(executions.Select(execution => execution.Task));
                }
                catch (Exception ex) {
                    Logger.LogError(ex, "One or more handlers threw while processing event [{EventId}]", domainEvent.EventId);
                }

                List<HandlerFailure> failures = executions.SelectMany(this.GetFailures).ToList();
                if (failures.Any())
                {
                    return Results.Problem(title: "One or more event handlers failed",
                                        statusCode: 500,
                                        extensions: new Dictionary<String, Object>
                                        {
                                            ["eventId"] = domainEvent.EventId,
                                            ["eventType"] = domainEvent.GetType().Name,
                                            ["failures"] = failures
                                        });
                }

                Logger.LogWarning("Finished processing event - ID [{EventId}]", domainEvent.EventId);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                String domainEventData = StringSerialiser.Serialise(domainEvent);
                Logger.LogError(ex,"Failed to Process Event Id {EventId}, Event Data received [{EventData}]", domainEvent.EventId, domainEventData);

                throw;
            }
        }

        [NonAction]
        public Task<IResult> PostEventAsync(Object request, CancellationToken cancellationToken)
        {
            String subscriptionId = this.Request.Headers.ContainsKey("subscriptionId")
                ? this.Request.Headers["subscriptionId"].ToString()
                : null;
            return this.PostEventAsync(request,
                                       cancellationToken,
                                       this.Request.Headers["eventType"].ToString(),
                                       subscriptionId);
        }

        private IDomainEvent SuccessFactory(IDomainEvent arg) {
            return arg;
        }

        private void Callback(CancellationToken cancellationToken,
                              Guid eventId)
        {
            if (cancellationToken.IsCancellationRequested) //I think this would always be true anyway
            {
                Logger.LogInformation("Cancel request for EventId {EventId}", eventId);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private IEnumerable<HandlerFailure> GetFailures(HandlerExecution execution)
        {
            if (execution.Task.IsFaulted)
            {
                String error = String.Join("; ", execution.Task.Exception?.Flatten().InnerExceptions
                    .SelectMany(exception => exception.GetExceptionMessages()) ?? Enumerable.Empty<String>());
                yield return new HandlerFailure(execution.Handler.GetType().Name, error);
            }
            else if (execution.Task.IsCanceled)
            {
                yield return new HandlerFailure(execution.Handler.GetType().Name, "Handler execution was cancelled");
            }
            else if (execution.Task.Result.IsFailed)
            {
                yield return new HandlerFailure(execution.Handler.GetType().Name, execution.Task.Result.Message);
            }
        }

        private HandlerExecution ExecuteHandler(IDomainEventHandler handler,
                                                IDomainEvent domainEvent,
                                                CancellationToken cancellationToken)
        {
            try
            {
                return new HandlerExecution(handler, handler.Handle(domainEvent, cancellationToken));
            }
            catch (Exception ex)
            {
                return new HandlerExecution(handler, Task.FromException<Result>(ex));
            }
        }

        private List<IDomainEventHandler> GetDomainEventHandlers(IDomainEvent domainEvent, String subscriptionId) {

            if (subscriptionId != null) {
                var resolver = this.Container.GetInstance<IDomainEventHandlerResolver>(subscriptionId);
                // We are being told by the caller to use a specific handler
                var allhandlers = resolver.GetDomainEventHandlers(domainEvent);

                if (allhandlers.IsFailed)
                    return new List<IDomainEventHandler>();

                return allhandlers.Data.ToList();
            }


            var eventHandlersResult = this.DomainEventHandlerResolver.GetDomainEventHandlers(domainEvent);
            if (eventHandlersResult.IsFailed)
                return new List<IDomainEventHandler>();
            return eventHandlersResult.Data;
        }

        private async Task<Result<IDomainEvent>> GetDomainEvent(Object domainEvent, String eventType)
        {
            Type type;

            try {
                type = TypeMap.GetType(eventType);
            }
            catch (Exception ex) {
                // Just swallowing exception here...
                type = null;
            }

            if (type == null)
                return Result.NotFound($"Failed to find a domain event with type {eventType}");
            
            if (type.IsSubclassOf(typeof(DomainEvent)))
            {
                String json = StringSerialiser.Serialise(domainEvent);

                DomainEventFactory domainEventFactory = new();
                return Result.Success(domainEventFactory.CreateDomainEvent(json, type) as IDomainEvent);
            }

            return null;
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        private const String ControllerName = "domainevents";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + DomainEventController.ControllerName;

        private sealed record HandlerExecution(IDomainEventHandler Handler, Task<Result> Task);

        private sealed record HandlerFailure(String Handler, String Error);

        #endregion
    }
}
