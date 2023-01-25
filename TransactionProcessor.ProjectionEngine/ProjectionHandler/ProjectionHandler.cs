namespace TransactionProcessor.ProjectionEngine.ProjectionHandler;

using System.Diagnostics;
using System.Text;
using Dispatchers;
using Projections;
using Repository;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.Logger;
using State;

public class ProjectionHandler<TState> : IProjectionHandler where TState : State
{
    private readonly IProjectionStateRepository<TState> ProjectionStateRepository;
    private readonly IProjection<TState> Projection;
    private readonly IStateDispatcher<TState> StateDispatcher;

    public ProjectionHandler(IProjectionStateRepository<TState> projectionStateRepository,
                             IProjection<TState> projection,
        IStateDispatcher<TState> stateDispatcher)
    {
        this.ProjectionStateRepository = projectionStateRepository;
        this.Projection = projection;
        this.StateDispatcher = stateDispatcher;
    }

    public async Task Handle(IDomainEvent @event, CancellationToken cancellationToken)
    {
        if (@event == null) return;

        if (this.Projection.ShouldIHandleEvent(@event) == false)
        {
            return;
        }
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        StringBuilder builder = new();

        //Load the state from persistence
        TState state = await this.ProjectionStateRepository.Load(@event, cancellationToken);

        if (state == null)
        {
            return;
        }

        builder.Append($"{stopwatch.ElapsedMilliseconds}ms After Load|");

        builder.Append($"{stopwatch.ElapsedMilliseconds}ms Handling {@event.EventType} for state {state.GetType().Name}|");

        TState newState = await this.Projection.Handle(state, @event, cancellationToken);

        builder.Append($"{stopwatch.ElapsedMilliseconds}ms After Handle|");

        if (newState != state)
        {
            newState = newState with
                       {
                           ChangesApplied = true
                       };

            // save state
            newState = await this.ProjectionStateRepository.Save(newState, @event, cancellationToken);

            //Repo might have detected a duplicate event
            builder.Append($"{stopwatch.ElapsedMilliseconds}ms After Save|");

            if (this.StateDispatcher != null)
            {
                //Send to anyone else interested
                await this.StateDispatcher.Dispatch(newState, @event, cancellationToken);

                builder.Append($"{stopwatch.ElapsedMilliseconds}ms After Dispatch|");
            }
            
        }
        else
        {
            builder.Append($"{stopwatch.ElapsedMilliseconds}ms No Save required|");
        }

        stopwatch.Stop();

        builder.Insert(0, $"Total time: {stopwatch.ElapsedMilliseconds}ms|");
        Logger.LogWarning(builder.ToString());

    }
}