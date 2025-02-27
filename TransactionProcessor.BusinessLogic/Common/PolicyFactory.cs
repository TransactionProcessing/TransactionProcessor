using System;
using System.Threading.Tasks;
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
    public static IAsyncPolicy<Result> CreatePolicy(Int32 retryCount=5, TimeSpan? retryDelay = null, String policyTag="", Boolean withFallBack=true) {

        TimeSpan retryDelayValue = retryDelay.GetValueOrDefault(TimeSpan.FromSeconds(1));

        AsyncRetryPolicy<Result> retryPolicy = CreateRetryPolicy(retryCount, retryDelayValue, policyTag);

        IAsyncPolicy<Result> policyWrap = withFallBack switch {
            true => CreateFallbackPolicy(policyTag, retryPolicy),
            _ => retryPolicy
        };
        return policyWrap;
    }

    public static async Task<Result> ExecuteWithPolicyAsync(Func<Task<Result>> action, IAsyncPolicy<Result> policy, String policyTag = "")
    {
        var context = new Context();
        context["RetryCount"] = 0;
        //Result result = await policy.ExecuteAsync(action);
        Result result = await policy.ExecuteAsync((ctx) => action(), context);
        int retryCount = (int)context["RetryCount"];
        String message = result.IsSuccess ? "Execution succeeded." : $"Execution failed with error: {result.Message}";
        String retryMessage = retryCount > 0 ? $" after {retryCount} retries." : "";
        // Log success if no retries were required

        Logger.LogWarning($"{policyTag} - {message} {retryMessage}");

        return result;
    }

    private static AsyncRetryPolicy<Result> CreateRetryPolicy(int retryCount, TimeSpan retryDelay, String policyTag)
    {
        return Policy<Result>
            .Handle<WrongExpectedVersionException>()
            .Or<RpcException>(ex => ex.StatusCode == StatusCode.DeadlineExceeded)
            .WaitAndRetryAsync(retryCount,
                _ => retryDelay, // Fixed delay
                (exception, timeSpan, retryCount, context) =>
                {
                    context["RetryCount"] = retryCount;
                    Logger.LogWarning($"{policyTag} - Retry {retryCount} due to {exception.Exception.GetType().Name}. Waiting {timeSpan} before retrying...");
                });
    }

    private static AsyncPolicyWrap<Result> CreateFallbackPolicy(String policyTag, AsyncRetryPolicy<Result> retryPolicy)
    {
        var fallbackPolicy = Policy<Result>
            .Handle<Exception>() // Catch-all for exceptions that aren't retried
            .FallbackAsync(
                fallbackValue: Result.Failure("An error occurred, no retry required."), // Ensure a valid Result return
                onFallbackAsync: (exception, context) =>
                {
                    Logger.LogWarning($"{policyTag} - Non-retryable exception encountered: {exception.Exception.GetType().Name}");
                    return Task.CompletedTask;
                });

        return fallbackPolicy.WrapAsync(retryPolicy);
    }
}