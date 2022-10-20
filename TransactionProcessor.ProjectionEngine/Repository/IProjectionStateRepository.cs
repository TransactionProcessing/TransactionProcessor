namespace TransactionProcessor.ProjectionEngine.Repository;

using Shared.DomainDrivenDesign.EventSourcing;
using State;

public interface IProjectionStateRepository<TState> where TState : State
{
    Task<TState> Load(IDomainEvent @event, CancellationToken cancellationToken);

    Task<TState> Load(Guid estateId, Guid stateId, CancellationToken cancellationToken);

    Task<TState> Save(TState state, IDomainEvent @event, CancellationToken cancellationToken);
}