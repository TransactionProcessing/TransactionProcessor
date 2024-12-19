namespace TransactionProcessor.ProjectionEngine.Projections;

using System.Diagnostics.Contracts;
using Database;
using Shared.DomainDrivenDesign.EventSourcing;
using State;

public interface IProjection<TState> where TState : Shared.EventStore.ProjectionEngine.State
{
    [Pure]
    Task<TState> Handle(TState state, IDomainEvent domainEvent, CancellationToken cancellationToken);

    bool ShouldIHandleEvent(IDomainEvent domainEvent);
}