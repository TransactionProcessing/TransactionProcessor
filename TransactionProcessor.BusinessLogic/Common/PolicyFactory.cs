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
public static class PolicyFactory
{
    private enum LogType
    {
        Retry,
        Final
    }

    public static IAsyncPolicy<Result> CreatePolicy(
        int retryCount = 5,
        TimeSpan? retryDelay = null,
        string policyTag = "",
        bool withFallBack = false)
    {
        TimeSpan delay = retryDelay.GetValueOrDefault(TimeSpan.FromSeconds(5));
        return CreateRetryPolicy(retryCount, delay, policyTag);
    }

    public static IAsyncPolicy<Result<T>> CreatePolicy<T>(
        int retryCount = 5,
        TimeSpan? retryDelay = null,
        string policyTag = "",
        bool withFallBack = false)
    {
        TimeSpan delay = retryDelay.GetValueOrDefault(TimeSpan.FromSeconds(5));
        return CreateRetryPolicy<T>(retryCount, delay, policyTag);
    }

    public static async Task<Result> ExecuteWithPolicyAsync(
        Func<Task<Result>> action,
        IAsyncPolicy<Result> policy,
        string policyTag = "")
    {
        var context = new Context();
        Result result = await policy.ExecuteAsync(ctx => action(), context);

        int retryCount = context.TryGetValue("RetryCount", out var retryObj) && retryObj is int r ? r : 0;
        LogResult(policyTag, result, retryCount, LogType.Final);

        return result;
    }

    public static async Task<Result<T>> ExecuteWithPolicyAsync<T>(
        Func<Task<Result<T>>> action,
        IAsyncPolicy<Result<T>> policy,
        string policyTag = "")
    {
        var context = new Context();
        Result<T> result = await policy.ExecuteAsync(ctx => action(), context);

        int retryCount = context.TryGetValue("RetryCount", out var retryObj) && retryObj is int r ? r : 0;
        LogResult(policyTag, result, retryCount, LogType.Final);

        return result;
    }

    private static AsyncRetryPolicy<Result> CreateRetryPolicy(
        int retryCount,
        TimeSpan retryDelay,
        string policyTag)
    {
        return Policy<Result>
            .HandleResult(ShouldRetry)
            .WaitAndRetryAsync(
                retryCount,
                _ => retryDelay,
                (result, timeSpan, attempt, context) =>
                {
                    context["RetryCount"] = attempt;
                    LogResult(policyTag, result.Result, attempt, LogType.Retry);
                });
    }

    private static AsyncRetryPolicy<Result<T>> CreateRetryPolicy<T>(
        int retryCount,
        TimeSpan retryDelay,
        string policyTag)
    {
        return Policy<Result<T>>
            .HandleResult(ShouldRetry)
            .WaitAndRetryAsync(
                retryCount,
                _ => retryDelay,
                (result, timeSpan, attempt, context) =>
                {
                    context["RetryCount"] = attempt;
                    LogResult(policyTag, result.Result, attempt, LogType.Retry);
                });
    }

    private static bool ShouldRetry(ResultBase result)
    {
        return !result.IsSuccess && result.Errors.Any(e =>
            e.Contains("WrongExpectedVersion", StringComparison.OrdinalIgnoreCase) ||
            e.Contains("DeadlineExceeded", StringComparison.OrdinalIgnoreCase) ||
            e.Contains("Cancelled"));
    }

    private static string FormatResultMessage(ResultBase result)
    {
        return result switch
        {
            { IsSuccess: true } => "Success",
            { IsSuccess: false, Message: not "" } => result.Message,
            { IsSuccess: false, Errors: var errors } when errors?.Any() == true => string.Join(", ", errors),
            _ => "Unknown Error"
        };
    }

    private static void LogResult(string policyTag, ResultBase result, int retryCount, LogType type)
    {
        string message = FormatResultMessage(result);

        switch (type)
        {
            case LogType.Retry:
                Logger.LogWarning($"{policyTag} - Retry {retryCount} due to error: {message}. Waiting before retrying...");
                break;

            case LogType.Final:
                string retryMessage = retryCount > 0 ? $" after {retryCount} retries." : "";
                Logger.LogWarning($"{policyTag} - {message}{retryMessage}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
                break;
        }
    }
}
