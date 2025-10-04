using Shared.Results;
using SimpleResults;

namespace TransactionProcessor.ProjectionEngine.ProjectionHandler;

using System.Diagnostics;
using System.Text;
using Dispatchers;
using Projections;
using Repository;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.Logger;

public class ProjectionHandler<TState> : IProjectionHandler where TState : Shared.EventStore.ProjectionEngine.State
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

    public async Task<Result> Handle(IDomainEvent @event, CancellationToken cancellationToken){
        if (@event == null) return Result.Success();

        Logger.LogInformation($"{@event.EventId}|In ProjectionHandler for {@event.EventType}");

        if (this.Projection.ShouldIHandleEvent(@event) == false){
            Logger.LogInformation($"{@event.EventId}|Silent Handle {@event.EventType}");
            return Result.Success();
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        StringBuilder builder = new();

        //Load the state from persistence
        Result<TState> loadResult = await this.ProjectionStateRepository.Load(@event, cancellationToken);

        if (loadResult.IsFailed) {
            return ResultHelpers.CreateFailure(loadResult);
        }
        Logger.LogInformation($"{@event.EventId}|State loaded for {@event.EventType}");
        var state = loadResult.Data;
        builder.Append($"{stopwatch.ElapsedMilliseconds}ms After Load|");

        builder.Append($"{stopwatch.ElapsedMilliseconds}ms Handling {@event.EventType} Id [{@event.EventId}] for state {state.GetType().Name}|");

        TState newState = await this.Projection.Handle(state, @event, cancellationToken);
        Logger.LogInformation($"{@event.EventId}|Event handled {@event.EventType}");
        builder.Append($"{stopwatch.ElapsedMilliseconds}ms After Handle|");

        if (newState != state){
            newState = newState with{
                                        ChangesApplied = true
                                    };

            // save state
            var saveResult = await this.ProjectionStateRepository.Save(newState, @event, cancellationToken);
            if (saveResult.IsFailed) {
                return ResultHelpers.CreateFailure(saveResult);
            }
            Logger.LogInformation($"{@event.EventId}|Event changes saved {@event.EventType}");
            //Repo might have detected a duplicate event
            builder.Append($"{stopwatch.ElapsedMilliseconds}ms After Save|");

            if (this.StateDispatcher != null){
                // Send to anyone else interested
                Result dispatchResult = await this.StateDispatcher.Dispatch(newState, @event, cancellationToken);
                if (dispatchResult.IsFailed) {
                    return ResultHelpers.CreateFailure(dispatchResult);
                }

                Logger.LogInformation($"{@event.EventId}|Event changes dispatched {@event.EventType}");
                builder.Append($"{stopwatch.ElapsedMilliseconds}ms After Dispatch|");
            }

        }
        else{
            builder.Append($"{@event.EventId}|{stopwatch.ElapsedMilliseconds}ms No Save required|");
        }

        stopwatch.Stop();

        builder.Insert(0, $"Total time: {stopwatch.ElapsedMilliseconds}ms|");

        Logger.LogWarning(builder.ToString());
        Logger.LogInformation($"{@event.EventId}|Event Type {@event.EventType} Id [{@event.EventId}] for state {state.GetType().Name} took {stopwatch.ElapsedMilliseconds}ms to process");
        
        return Result.Success();
    }
}