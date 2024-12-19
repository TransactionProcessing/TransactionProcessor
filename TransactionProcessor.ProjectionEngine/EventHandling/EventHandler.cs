using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.EventHandling
{
    using System.Diagnostics.CodeAnalysis;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.EventHandling;
    using System.Reflection;
    using Database;
    using Microsoft.AspNetCore.Hosting;
    using Shared.General;
    using State;

    [ExcludeFromCodeCoverage]
    public class EventHandler : IDomainEventHandler
    {
        private readonly Func<String, IDomainEventHandler> Resolver;

        public static Dictionary<String, Type> StateTypes;

        public EventHandler(Func<String, IDomainEventHandler> resolver)
        {
            this.Resolver = resolver;
            List<Type> subclassTypes = Assembly.GetAssembly(typeof(Shared.EventStore.ProjectionEngine.State))?.GetTypes().Where(t => t.IsSubclassOf(typeof(Shared.EventStore.ProjectionEngine.State))).ToList();

            if (subclassTypes != null)
            {
                EventHandler.StateTypes = subclassTypes.ToDictionary(x => x.Name, x => x);
            }
        }
        
        public async Task<Result> Handle(IDomainEvent domainEvent,
                                         CancellationToken cancellationToken) {
            // Lookup the event type in the config
            String? eventType  = ConfigurationReader.GetValue("AppSettings:EventStateConfig", domainEvent.GetType().Name);

            IDomainEventHandler handler = this.Resolver(eventType);

            return await handler.Handle(domainEvent, cancellationToken);
        }
    }
}
