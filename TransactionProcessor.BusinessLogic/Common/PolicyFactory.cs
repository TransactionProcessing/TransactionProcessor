using System;
using System.Threading.Tasks;
using Polly;
using TransactionProcessing.UnionResults;

namespace TransactionProcessor.BusinessLogic.Common;

/// <summary>Creates the retry policy used by event handlers and domain services.</summary>
public static class PolicyFactory
{
    public static IAsyncPolicy<Result> CreatePolicy(string policyTag) =>
        CreatePolicy<Result>(result => result.IsSuccess() == false);

    public static IAsyncPolicy<Result<T>> CreatePolicy<T>(string policyTag) =>
        CreatePolicy<Result<T>>(result => result.IsSuccess() == false);

    private static IAsyncPolicy<TResult> CreatePolicy<TResult>(Func<TResult, bool> isFailed) =>
        Policy.Handle<Exception>()
              .OrResult(isFailed)
              .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    public static Task<TResult> ExecuteWithPolicyAsync<TResult>(Func<Task<TResult>> operation,
                                                                 IAsyncPolicy<TResult> policy,
                                                                 string policyTag)
    {
        return policy.ExecuteAsync(operation);
    }
}
