using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace TransactionProcessor.IntegrationTests.Common;

public class RetryTests
{
    [Test]
    public async Task For_DoesNotBlockWhileWaitingToRetry()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Task retryTask = Retry.For(
            () => Task.FromException(new InvalidOperationException("expected failure")),
            retryFor: TimeSpan.FromMilliseconds(50),
            retryInterval: TimeSpan.FromSeconds(1));

        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(500));

        await Should.ThrowAsync<InvalidOperationException>(async () => await retryTask);
    }
}
