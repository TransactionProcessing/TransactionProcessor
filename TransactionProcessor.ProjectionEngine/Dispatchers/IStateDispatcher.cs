using TransactionProcessing.UnionResults;

namespace TransactionProcessor.ProjectionEngine.Dispatchers{
    using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;

    public interface IStateDispatcher<in TState> where TState : TransactionProcessing.Shared.EventStore.ProjectionEngine.State{
        Task<Result> Dispatch(TState state, IDomainEvent @event, CancellationToken cancellationToken);
    }
}