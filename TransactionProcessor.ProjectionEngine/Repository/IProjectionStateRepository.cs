using TransactionProcessing.UnionResults;

namespace TransactionProcessor.ProjectionEngine.Repository;

using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;

public interface IProjectionStateRepository<TState> where TState : TransactionProcessing.Shared.EventStore.ProjectionEngine.State
{
    Task<Result<TState>> Load(IDomainEvent @event, CancellationToken cancellationToken);

    Task<Result<TState>> Load(Guid estateId, Guid stateId, CancellationToken cancellationToken);

    Task<Result<TState>> Save(TState state, IDomainEvent @event, CancellationToken cancellationToken);
}