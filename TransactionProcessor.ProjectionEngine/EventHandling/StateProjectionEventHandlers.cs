using Shared.Exceptions;
using Shared.Logger;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.EventHandling;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Database.Database;
using ProjectionHandler;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.EventHandling;

[ExcludeFromCodeCoverage]
public class StateProjectionEventHandler<TState> : IDomainEventHandler where TState : Shared.EventStore.ProjectionEngine.State
{
    private readonly IProjectionHandler ProjectionHandler;
    private readonly IDbContextResolver<EstateManagementContext> Resolver;
    private static readonly String EstateManagementDatabaseName = "TransactionProcessorReadModel";

    public StateProjectionEventHandler(ProjectionHandler<TState> projectionHandler,
                                       IDbContextResolver<EstateManagementContext> resolver) {
        this.ProjectionHandler = projectionHandler;
        this.Resolver = resolver;
    }
    
    #region Methods

    public async Task<Result> Handle(IDomainEvent domainEvent,
                                     CancellationToken cancellationToken) {
        if (domainEvent.GetType() == typeof(EstateDomainEvents.EstateCreatedEvent)) {
            return await this.MigrateDatabase((EstateDomainEvents.EstateCreatedEvent)domainEvent, cancellationToken);
        }

        Logger.LogWarning($"|{domainEvent.EventId}|State Projection Domain Event Handler - Inside Handle {domainEvent.EventType}");
        Stopwatch sw = Stopwatch.StartNew();
        Result result = await this.ProjectionHandler.Handle(domainEvent, cancellationToken);
        sw.Stop();
        Logger.LogWarning($"|{domainEvent.EventId}|State Projection Event Handler - after Handle {domainEvent.EventType} time {sw.ElapsedMilliseconds}ms");

        return result;
    }

    private async Task<Result> MigrateDatabase(EstateDomainEvents.EstateCreatedEvent domainEvent, CancellationToken cancellationToken) {
        try {
            using ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, domainEvent.EstateId.ToString());
            await using EstateManagementContext context = resolvedContext.Context;
            await context.MigrateAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex) {
            return Result.CriticalError(ex.GetExceptionMessages());
        }
    }

    #endregion
}