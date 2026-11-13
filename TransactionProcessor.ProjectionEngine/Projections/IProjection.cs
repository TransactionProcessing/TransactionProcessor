namespace TransactionProcessor.ProjectionEngine.Projections;

using System.Diagnostics.Contracts;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;

public interface IProjection<TState> where TState : TransactionProcessing.Shared.EventStore.ProjectionEngine.State
{
    [Pure]
    Task<TState> Handle(TState state, IDomainEvent domainEvent, CancellationToken cancellationToken);

    bool ShouldIHandleEvent(IDomainEvent domainEvent);
}