namespace TransactionProcessor.ProjectionEngine.Dispatchers;

using Shared.DomainDrivenDesign.EventSourcing;
using State;

public interface IStateDispatcher<in TState> where TState : State
{
    Task Dispatch(TState state, IDomainEvent @event, CancellationToken cancellationToken);
}