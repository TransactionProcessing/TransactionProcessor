using Shared.Exceptions;
using Shared.Logger;
using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.EventHandling;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Database;
using Database.Database;
using EstateManagement.Estate.DomainEvents;
using ProjectionHandler;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.EventHandling;
using State;

[ExcludeFromCodeCoverage]
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

    public async Task<Result> Handle(IDomainEvent domainEvent,
                                     CancellationToken cancellationToken) {
        if (domainEvent.GetType() == typeof(EstateCreatedEvent)) {
            return await this.MigrateDatabase((EstateCreatedEvent)domainEvent, cancellationToken);
        }

        Logger.LogWarning($"|{domainEvent.EventId}|State Projection Domain Event Handler - Inside Handle {domainEvent.EventType}");
        Stopwatch sw = Stopwatch.StartNew();
        var result = await this.ProjectionHandler.Handle(domainEvent, cancellationToken);
        sw.Stop();
        Logger.LogWarning($"|{domainEvent.EventId}|State Projection Event Handler - after Handle {domainEvent.EventType} time {sw.ElapsedMilliseconds}ms");

        return result;
    }

    private async Task<Result> MigrateDatabase(EstateCreatedEvent domainEvent, CancellationToken cancellationToken) {
        try {
            TransactionProcessorGenericContext? context = await this.ContextFactory.GetContext(domainEvent.EstateId,
                "TransactionProcessorReadModel", cancellationToken);
            await context.MigrateAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) {
            return Result.CriticalError(ex.GetExceptionMessages());
        }
    }

    #endregion
}