using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransactionProcessor.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using MessagingService.BusinessLogic.EventHandling;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.Logger;

    [Route(DomainEventController.ControllerRoute)]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class DomainEventController : ControllerBase
    {
        private readonly IDomainEventHandlerResolver DomainEventHandlerResolver;

        public DomainEventController(IDomainEventHandlerResolver domainEventHandlerResolver)
        {
            this.DomainEventHandlerResolver = domainEventHandlerResolver;
        }

        /// <summary>
        /// Posts the event asynchronous.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> PostEventAsync([FromBody] DomainEvent domainEvent,
                                                        CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => this.Callback(cancellationToken, domainEvent.EventId));

            try
            {
                Logger.LogInformation($"Processing event - ID [{domainEvent.EventId}], Type[{domainEvent.GetType().Name}]");

                List<IDomainEventHandler> eventHandlers = this.DomainEventHandlerResolver.GetDomainEventHandlers(domainEvent);

                if (eventHandlers == null || eventHandlers.Any() == false)
                {
                    // Log a warning out 
                    Logger.LogWarning($"No event handlers configured for Event Type [{domainEvent.GetType().Name}]");
                    return this.Ok();
                }

                List<Task> tasks = new List<Task>();
                foreach (IDomainEventHandler domainEventHandler in eventHandlers)
                {
                    tasks.Add(domainEventHandler.Handle(domainEvent, cancellationToken));
                }

                Task.WaitAll(tasks.ToArray());

                Logger.LogInformation($"Finished processing event - ID [{domainEvent.EventId}]");

                return this.Ok();
            }
            catch (Exception ex)
            {
                String domainEventData = JsonConvert.SerializeObject(domainEvent);
                Logger.LogError(new Exception($" Failed to Process Event, Event Data received [{domainEventData}]", ex));

                throw;
            }
        }

        /// <summary>
        /// Callbacks the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="eventId">The event identifier.</param>
        private void Callback(CancellationToken cancellationToken,
                              Guid eventId)
        {
            if (cancellationToken.IsCancellationRequested) //I think this would always be true anyway
            {
                Logger.LogInformation($"Cancel request for EventId {eventId}");
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "domainevents";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + DomainEventController.ControllerName;

        #endregion

    }
}
