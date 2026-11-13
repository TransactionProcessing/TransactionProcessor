using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.UnionResults;

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
            _ => Result.Success(result.GetValueOrThrow())
        };
    }

    public static Result<T> HandleGetAggregateResult<T>(Result<T> result, Guid aggregateId, bool isNotFoundError = true)
        where T : Aggregate, new()  // Constraint: T is a subclass of Aggregate and has a parameterless constructor
    {
        bool isNotFound = result.Errors.Any(error => error.ErrorType == ErrorType.NotFound);
        if (result.IsFailed && !isNotFound)
        {
            return ResultHelpers.CreateFailure(result);
        }

        if (isNotFound && isNotFoundError)
        {
            return ResultHelpers.CreateFailure(result);
        }

        T aggregate = isNotFound ? new T { AggregateId = aggregateId } : result.GetValueOrThrow();

        return Result.Success(aggregate);
    }
}
