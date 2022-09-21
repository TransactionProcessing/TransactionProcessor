namespace TransactionProcessor
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using EventStore.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Shared.EventStore.EventHandling;
    using Shared.EventStore.SubscriptionWorker;
    using Shared.General;
    using Shared.Logger;
    using TransactionProcessor.BusinessLogic.OperatorInterfaces;

    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        public static IServiceCollection AddInSecureEventStoreClient(this IServiceCollection services,
                                                                     Uri address,
                                                                     Func<HttpMessageHandler>? createHttpMessageHandler = null)
        {
            return services.AddEventStoreClient((Action<EventStoreClientSettings>)(options => {
                                                                                       options.ConnectivitySettings.Address = address;
                                                                                       options.ConnectivitySettings.Insecure = true;
                                                                                       options.CreateHttpMessageHandler = createHttpMessageHandler;
                                                                                   }));
        }

        static Action<TraceEventType, String, String> log = (tt, subType, message) => {
                                                                String logMessage = $"{subType} - {message}";
                                                                switch (tt)
                                                                {
                                                                    case TraceEventType.Critical:
                                                                        Logger.LogCritical(new Exception(logMessage));
                                                                        break;
                                                                    case TraceEventType.Error:
                                                                        Logger.LogError(new Exception(logMessage));
                                                                        break;
                                                                    case TraceEventType.Warning:
                                                                        Logger.LogWarning(logMessage);
                                                                        break;
                                                                    case TraceEventType.Information:
                                                                        Logger.LogInformation(logMessage);
                                                                        break;
                                                                    case TraceEventType.Verbose:
                                                                        Logger.LogDebug(logMessage);
                                                                        break;
                                                                }
                                                            };

        static Action<TraceEventType, String> concurrentLog = (tt, message) => Extensions.log(tt, "CONCURRENT", message);

        public static void PreWarm(this IApplicationBuilder applicationBuilder)
        {
            Startup.LoadTypes();

            //SubscriptionWorker worker = new SubscriptionWorker()
            var internalSubscriptionService = Boolean.Parse(ConfigurationReader.GetValue("InternalSubscriptionService"));

            if (internalSubscriptionService)
            {
                String eventStoreConnectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");
                Int32 inflightMessages = Int32.Parse(ConfigurationReader.GetValue("AppSettings", "InflightMessages"));
                Int32 persistentSubscriptionPollingInSeconds = Int32.Parse(ConfigurationReader.GetValue("AppSettings", "PersistentSubscriptionPollingInSeconds"));
                String filter = ConfigurationReader.GetValue("AppSettings", "InternalSubscriptionServiceFilter");
                String ignore = ConfigurationReader.GetValue("AppSettings", "InternalSubscriptionServiceIgnore");
                String streamName = ConfigurationReader.GetValue("AppSettings", "InternalSubscriptionFilterOnStreamName");
                Int32 cacheDuration = Int32.Parse(ConfigurationReader.GetValue("AppSettings", "InternalSubscriptionServiceCacheDuration"));

                ISubscriptionRepository subscriptionRepository = SubscriptionRepository.Create(eventStoreConnectionString, cacheDuration);

                ((SubscriptionRepository)subscriptionRepository).Trace += (sender, s) => Extensions.log(TraceEventType.Information, "REPOSITORY", s);
                
                // init our SubscriptionRepository
                subscriptionRepository.PreWarm(CancellationToken.None).Wait();

                var eventHandlerResolver = Startup.ServiceProvider.GetService<IDomainEventHandlerResolver>();

                SubscriptionWorker concurrentSubscriptions = SubscriptionWorker.CreateConcurrentSubscriptionWorker(Startup.EventStoreClientSettings, eventHandlerResolver, subscriptionRepository, inflightMessages, persistentSubscriptionPollingInSeconds);

                concurrentSubscriptions.Trace += (_, args) => Extensions.concurrentLog(TraceEventType.Information, args.Message);
                concurrentSubscriptions.Warning += (_, args) => Extensions.concurrentLog(TraceEventType.Warning, args.Message);
                concurrentSubscriptions.Error += (_, args) => Extensions.concurrentLog(TraceEventType.Error, args.Message);

                if (!String.IsNullOrEmpty(ignore))
                {
                    concurrentSubscriptions = concurrentSubscriptions.IgnoreSubscriptions(ignore);
                }

                if (!String.IsNullOrEmpty(filter))
                {
                    //NOTE: Not overly happy with this design, but;
                    //the idea is if we supply a filter, this overrides ignore
                    concurrentSubscriptions = concurrentSubscriptions.FilterSubscriptions(filter)
                                                                     .IgnoreSubscriptions(null);

                }

                if (!String.IsNullOrEmpty(streamName))
                {
                    concurrentSubscriptions = concurrentSubscriptions.FilterByStreamName(streamName);
                }

                concurrentSubscriptions.StartAsync(CancellationToken.None).Wait();
            }

            if (Startup.AutoApiLogonOperators.Any())
            {
                foreach (String autoApiLogonOperator in Startup.AutoApiLogonOperators)
                {
                    OperatorLogon(autoApiLogonOperator);
                }
            }
        }

        private static void OperatorLogon(String operatorId)
        {

            Logger.LogInformation($"About to do auto logon for operator Id [{operatorId}]");
            Func<String, IOperatorProxy> resolver = Startup.ServiceProvider.GetService<Func<String, IOperatorProxy>>();
            IOperatorProxy proxy = resolver(operatorId);

            OperatorResponse logonResult = proxy.ProcessLogonMessage(null, CancellationToken.None).Result;
            Logger.LogInformation($"Auto logon for operator Id [{operatorId}] status [{logonResult.IsSuccessful}]");
        }
    }
}