using System;
using System.Reflection;
using System.Threading.Tasks;
using Polly;

namespace TransactionProcessor.BusinessLogic.Common;

/// <summary>Creates the retry policy used by event handlers and domain services.</summary>
public static class PolicyFactory
{
    public static IAsyncPolicy<TResult> CreatePolicy<TResult>(string policyTag)
    {
        return Policy
            .Handle<Exception>()
            .OrResult<TResult>(result => IsFailed(result))
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public static Task<TResult> ExecuteWithPolicyAsync<TResult>(Func<Task<TResult>> operation,
                                                                 IAsyncPolicy<TResult> policy,
                                                                 string policyTag)
    {
        return policy.ExecuteAsync(operation);
    }

    private static bool IsFailed<TResult>(TResult result)
    {
        PropertyInfo? isFailedProperty = result?.GetType().GetProperty("IsFailed", BindingFlags.Instance | BindingFlags.Public);
        return isFailedProperty?.GetValue(result) as bool? ?? false;
    }
}
