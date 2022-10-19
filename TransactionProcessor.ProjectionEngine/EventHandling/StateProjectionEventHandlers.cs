namespace TransactionProcessor.ProjectionEngine.EventHandling;

using Database;
using EstateManagement.Estate.DomainEvents;
using ProjectionHandler;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.EventHandling;
using State;

public class StateProjectionEventHandler<TState> : IDomainEventHandler where TState : State
{
    #region Fields

    private readonly IProjectionHandler ProjectionHandler;

    private readonly IDbContextFactory<TransactionProcessorGenericContext> ContextFactory;

    #endregion

    #region Constructors

    public StateProjectionEventHandler(ProjectionHandler<TState> projectionHandler,
                                       IDbContextFactory<TransactionProcessorGenericContext> contextFactory) {
        this.ProjectionHandler = projectionHandler;
        this.ContextFactory = contextFactory;
    }

    #endregion

    #region Methods

    public async Task Handle(IDomainEvent domainEvent,
                             CancellationToken cancellationToken) {
        if (domainEvent.GetType() == typeof(EstateCreatedEvent)) {
            await this.MigrateDatabase((EstateCreatedEvent)domainEvent, cancellationToken);
            return;
        }

        await this.ProjectionHandler.Handle(domainEvent, cancellationToken);
    }

    private async Task MigrateDatabase(EstateCreatedEvent domainEvent, CancellationToken cancellationToken) {
        var context = await this.ContextFactory.GetContext(domainEvent.EstateId, cancellationToken);
        await context.MigrateAsync(cancellationToken);
    }

    #endregion
}