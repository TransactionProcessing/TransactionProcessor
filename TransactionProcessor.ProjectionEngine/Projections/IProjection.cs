namespace TransactionProcessor.ProjectionEngine.Projections;

using System.Diagnostics.Contracts;
using Shared.DomainDrivenDesign.EventSourcing;

public interface IProjection<TState> where TState : Shared.EventStore.ProjectionEngine.State
{
    [Pure]
    Task<TState> Handle(TState state, IDomainEvent domainEvent, CancellationToken cancellationToken);

    bool ShouldIHandleEvent(IDomainEvent domainEvent);
}