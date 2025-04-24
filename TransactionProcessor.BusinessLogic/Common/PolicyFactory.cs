using EventStore.Client;
using Grpc.Core;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Wrap;
using Shared.Logger;
using SimpleResults;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Common;

[ExcludeFromCodeCoverage]
public static class PolicyFactory{
    public static IAsyncPolicy<Result> CreatePolicy(Int32 retryCount=5, TimeSpan? retryDelay = null, String policyTag="", Boolean withFallBack=false) {

        TimeSpan retryDelayValue = retryDelay.GetValueOrDefault(TimeSpan.FromSeconds(2));

        AsyncRetryPolicy<Result> retryPolicy = CreateRetryPolicy(retryCount, retryDelayValue, policyTag);

        return retryPolicy;
    }

    public static async Task<Result> ExecuteWithPolicyAsync(Func<Task<Result>> action, IAsyncPolicy<Result> policy, String policyTag = "")
    {
        var context = new Context();
        context["RetryCount"] = 0;

        Result result = await policy.ExecuteAsync((ctx) => action(), context);
        
        int retryCount = (int)context["RetryCount"];
        String message = result switch
        {
            { IsSuccess: true } => "Success",
            { IsSuccess: false, Message: not "" } => result.Message,
            { IsSuccess: false, Message: "", Errors: var errors } when errors.Any() => string.Join(", ", errors),
            _ => "Unknown Error"
        };
        String retryMessage = retryCount > 0 ? $" after {retryCount} retries." : "";
        // Log success if no retries were required

        Logger.LogWarning($"{policyTag} - {message} {retryMessage}");

        return result;
    }

    private static AsyncRetryPolicy<Result> CreateRetryPolicy(int retryCount, TimeSpan retryDelay, String policyTag)
    {
        return Policy<Result>
            .HandleResult(result => !result.IsSuccess && String.Join("|",result.Errors).Contains("Append failed due to WrongExpectedVersion")) // Retry if the result is not successful
            .OrResult(result => !result.IsSuccess && String.Join("|", result.Errors).Contains("DeadlineExceeded")) // Retry if the result is not successful
            .WaitAndRetryAsync(retryCount,
                _ => retryDelay, // Fixed delay
                (result, timeSpan, retryCount, context) =>
                {
                    context["RetryCount"] = retryCount;
                    Logger.LogWarning($"{policyTag} - Retry {retryCount} due to unsuccessful result  {String.Join(".",result.Result.Errors)}. Waiting {timeSpan} before retrying...");
                });

    }
}