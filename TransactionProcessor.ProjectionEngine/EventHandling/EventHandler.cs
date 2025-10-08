using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.EventHandling
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.EventHandling;
    using Shared.General;

    [ExcludeFromCodeCoverage]
    public class EventHandler : IDomainEventHandler
    {
        private readonly Func<String, IDomainEventHandler> Resolver;
        
        public EventHandler(Func<String, IDomainEventHandler> resolver)
        {
            this.Resolver = resolver;
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
