using System;
using System.Threading;
using System.Threading.Tasks;
using Shared.EventStore.Aggregate;
using Shared.Results;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Common;

public static class DomainServiceHelper
{
    public static async Task<Result<TAggregate>> GetAggregateOrFailure<TAggregate>(Func<CancellationToken, Task<Result<TAggregate>>> fetchFunc,
                                                                                   Guid aggregateId,
                                                                                   CancellationToken cancellationToken,
                                                                                   Boolean isNotFoundError = true) where TAggregate : Aggregate, new()
    {
        Result<TAggregate> result = await fetchFunc(cancellationToken);
        return result.IsFailed switch
        {
            true => DomainServiceHelper.HandleGetAggregateResult(result, aggregateId, isNotFoundError),
            _ => Result.Success(result.Data)
        };
    }

    public static Result<T> HandleGetAggregateResult<T>(Result<T> result, Guid aggregateId, bool isNotFoundError = true)
        where T : Aggregate, new()  // Constraint: T is a subclass of Aggregate and has a parameterless constructor
    {
        if (result.IsFailed && result.Status != ResultStatus.NotFound)
        {
            return ResultHelpers.CreateFailure(result);
        }

        if (result.Status == ResultStatus.NotFound && isNotFoundError)
        {
            return ResultHelpers.CreateFailure(result);
        }

        T aggregate = result.Status switch
        {
            ResultStatus.NotFound => new T { AggregateId = aggregateId },  // Set AggregateId when creating a new instance
            _ => result.Data
        };

        return Result.Success(aggregate);
    }
}