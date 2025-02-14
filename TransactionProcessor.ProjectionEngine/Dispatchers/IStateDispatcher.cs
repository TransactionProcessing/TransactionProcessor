using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.Dispatchers{
    using Shared.DomainDrivenDesign.EventSourcing;

    public interface IStateDispatcher<in TState> where TState : Shared.EventStore.ProjectionEngine.State{
        Task<Result> Dispatch(TState state, IDomainEvent @event, CancellationToken cancellationToken);
    }
}