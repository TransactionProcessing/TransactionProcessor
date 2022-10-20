using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.ProjectionEngine.EventHandling
{
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.EventHandling;
    using System.Reflection;
    using Microsoft.AspNetCore.Hosting;
    using Shared.General;
    using State;

    public class EventHandler : IDomainEventHandler
    {
        private readonly Func<String, IDomainEventHandler> Resolver;

        public static Dictionary<String, Type> StateTypes;

        public EventHandler(Func<String, IDomainEventHandler> resolver)
        {
            this.Resolver = resolver;
            List<Type> subclassTypes = Assembly.GetAssembly(typeof(State))?.GetTypes().Where(t => t.IsSubclassOf(typeof(State))).ToList();

            if (subclassTypes != null)
            {
                EventHandler.StateTypes = subclassTypes.ToDictionary(x => x.Name, x => x);
            }
        }
        
        public async Task Handle(IDomainEvent domainEvent,
                           CancellationToken cancellationToken) {
            // Lookup the event type in the config
            var gimp  = ConfigurationReader.GetValue("AppSettings:EventStateConfig", domainEvent.GetType().Name);

            var handler = this.Resolver(gimp);

            await handler.Handle(domainEvent, cancellationToken);

        }
    }
}
