using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.Repository;

using Shared.DomainDrivenDesign.EventSourcing;
using State;

public interface IProjectionStateRepository<TState> where TState : State
{
    Task<Result<TState>> Load(IDomainEvent @event, CancellationToken cancellationToken);

    Task<Result<TState>> Load(Guid estateId, Guid stateId, CancellationToken cancellationToken);

    Task<Result<TState>> Save(TState state, IDomainEvent @event, CancellationToken cancellationToken);
}