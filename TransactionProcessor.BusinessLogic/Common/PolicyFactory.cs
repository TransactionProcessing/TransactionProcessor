using System;
using EventStore.Client;
using Grpc.Core;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Wrap;
using Shared.Logger;
using SimpleResults;

namespace TransactionProcessor.BusinessLogic.Common;

public static class PolicyFactory{
    public static IAsyncPolicy<Result> CreatePolicy(Int32 retryCount=5, TimeSpan? retryDelay = null, String policyTag="", Boolean withFallBack=false) {

        TimeSpan retryDelayValue = retryDelay.GetValueOrDefault(TimeSpan.FromSeconds(1));

        AsyncRetryPolicy<Result> retryPolicy = CreateRetryPolicy(retryCount, retryDelayValue, policyTag);

        return withFallBack switch {
            true => CreateFallbackPolicy(policyTag, retryPolicy),
            _ => retryPolicy
        };
    }

    private static AsyncRetryPolicy<Result> CreateRetryPolicy(int retryCount, TimeSpan retryDelay, String policyTag)
    {
        //
        return Policy<Result>
            .Handle<WrongExpectedVersionException>()
            .Or<RpcException>(ex => ex.StatusCode == StatusCode.DeadlineExceeded)
            .WaitAndRetryAsync(retryCount,
                _ => retryDelay, // Fixed delay
                (exception, timeSpan, retryCount, context) =>
                {
                    Logger.LogWarning($"{policyTag} - Retry {retryCount} due to {exception.GetType().Name}. Waiting {timeSpan} before retrying...");
                });
    }

    private static AsyncPolicyWrap<Result> CreateFallbackPolicy(String policyTag, AsyncRetryPolicy<Result> retryPolicy)
    {
        AsyncFallbackPolicy<Result> fallbackPolicy = Policy<Result>
            .Handle<WrongExpectedVersionException>()
            .Or<TimeoutException>()
            .FallbackAsync(async (cancellationToken) =>
            {
                Logger.LogWarning($"{policyTag}  -All retries failed. Executing fallback action...");
                // Log failure, notify monitoring system, or enqueue for later processing
                return Result.Failure("Fallback action executed");
            });

        return fallbackPolicy.WrapAsync(retryPolicy);
    }
}