using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TransactionProcessor
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using EventStore.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Reconciliation.DomainEvents;
    using Settlement.DomainEvents;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.EventStore.Subscriptions;
    using Shared.Logger;
    using Transaction.DomainEvents;

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            Program.CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Console.Title = "Transaction Processor";

            //At this stage, we only need our hosting file for ip and ports
            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                                  .AddJsonFile("hosting.json", optional: true)
                                                                  .AddJsonFile("hosting.development.json", optional: true)
                                                                  .AddEnvironmentVariables().Build();

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                                                 {
                                                     webBuilder.UseStartup<Startup>();
                                                     webBuilder.UseConfiguration(config);
                                                     webBuilder.UseKestrel();
                                                 })
                       .ConfigureServices(services =>
                                          {
                                              PendingSettlementCreatedForDateEvent s =
                                                  new PendingSettlementCreatedForDateEvent(Guid.Parse("62CA5BF0-D138-4A19-9970-A4F7D52DE292"),
                                                                                           Guid.Parse("3E42516B-6C6F-4F86-BF08-3EF0ACDDDD55"),
                                                                                           DateTime.Now);

                                              TransactionHasStartedEvent t = new TransactionHasStartedEvent(Guid.Parse("2AA2D43B-5E24-4327-8029-1135B20F35CE"), Guid.NewGuid(),Guid.NewGuid(), 
                                                                                                            DateTime.Now, "","","","",null);

                                              ReconciliationHasStartedEvent r =
                                                  new ReconciliationHasStartedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

                                              TypeProvider.LoadDomainEventsTypeDynamically();

                                              services.AddHostedService<SubscriptionWorker>(provider =>
                                                                                            {
                                                                                                IDomainEventHandlerResolver r =
                                                                                                    provider.GetRequiredService<IDomainEventHandlerResolver>();
                                                                                                EventStorePersistentSubscriptionsClient p = provider.GetRequiredService<EventStorePersistentSubscriptionsClient>();
                                                                                                HttpClient h = provider.GetRequiredService<HttpClient>();
                                                                                                SubscriptionWorker worker = new SubscriptionWorker(r, p, h);
                                                                                                worker.TraceGenerated += Worker_TraceGenerated;
                                                                                                return worker;
                                                                                            });
                                          });
            return hostBuilder;
        }

        /// <summary>
        /// Workers the trace generated.
        /// </summary>
        /// <param name="trace">The trace.</param>
        /// <param name="logLevel">The log level.</param>
        private static void Worker_TraceGenerated(string trace, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    Logger.LogTrace(trace);
                    break;
                case LogLevel.Debug:
                    Logger.LogDebug(trace);
                    break;
                case LogLevel.Information:
                    Logger.LogInformation(trace);
                    break;
                case LogLevel.Warning:
                    Logger.LogWarning(trace);
                    break;
                case LogLevel.Error:
                    Logger.LogError(new Exception(trace));
                    break;
                case LogLevel.Critical:
                    Logger.LogCritical(new Exception(trace));
                    break;
            }
        }
    }
}
