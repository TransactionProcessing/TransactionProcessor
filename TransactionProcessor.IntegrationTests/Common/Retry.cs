using System;

namespace TransactionProcessor.IntegrationTests
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class Retry
    {
        #region Fields

        /// <summary>
        /// The default retry for
        /// </summary>
        private static readonly TimeSpan DefaultRetryFor = TimeSpan.FromSeconds(90);

        /// <summary>
        /// The default retry interval
        /// </summary>
        private static readonly TimeSpan DefaultRetryInterval = TimeSpan.FromSeconds(15);

        #endregion

        #region Methods

        /// <summary>
        /// Fors the specified action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="retryFor">The retry for.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <returns></returns>
        public static async Task For(Func<Task> action,
                                     TimeSpan? retryFor = null,
                                     TimeSpan? retryInterval = null)
        {
            TimeSpan retryDuration = retryFor ?? Retry.DefaultRetryFor;
            TimeSpan retryDelay = retryInterval ?? Retry.DefaultRetryInterval;
            Stopwatch stopwatch = Stopwatch.StartNew();
            Exception lastException;

            while (true)
            {
                try
                {
                    await action().ConfigureAwait(false);
                    return;
                }
                catch (Exception e)
                {
                    lastException = e;
                }

                TimeSpan remaining = retryDuration - stopwatch.Elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    throw lastException;
                }

                TimeSpan delay = retryDelay < remaining ? retryDelay : remaining;
                await Task.Delay(delay).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
