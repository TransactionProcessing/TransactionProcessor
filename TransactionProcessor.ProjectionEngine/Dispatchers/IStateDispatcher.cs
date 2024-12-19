using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.Dispatchers{

    using Database;
    using Shared.DomainDrivenDesign.EventSourcing;
    using State;

    public interface IStateDispatcher<in TState> where TState : Shared.EventStore.ProjectionEngine.State{
        Task<Result> Dispatch(TState state, IDomainEvent @event, CancellationToken cancellationToken);
    }
}