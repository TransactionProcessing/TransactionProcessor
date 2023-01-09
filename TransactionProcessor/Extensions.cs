namespace TransactionProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using EventStore.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
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

        static Action<TraceEventType, String> mainLog = (tt, message) => Extensions.log(tt, "MAIN", message);
        static Action<TraceEventType, String> orderedLog = (tt, message) => Extensions.log(tt, "ORDERED", message);

        public static void PreWarm(this IApplicationBuilder applicationBuilder) {
            Startup.LoadTypes();

            IConfigurationSection subscriptionConfigSection = Startup.Configuration.GetSection("AppSettings:SubscriptionConfiguration");
            SubscriptionWorkersRoot subscriptionWorkersRoot = new SubscriptionWorkersRoot();
            subscriptionConfigSection.Bind(subscriptionWorkersRoot);

            if (subscriptionWorkersRoot.InternalSubscriptionService) {
                
                String eventStoreConnectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");
                
                ISubscriptionRepository subscriptionRepository = SubscriptionRepository.Create(eventStoreConnectionString, subscriptionWorkersRoot.InternalSubscriptionServiceCacheDuration);
                ((SubscriptionRepository)subscriptionRepository).Trace += (sender,
                                                                           s) => Extensions.log(TraceEventType.Information, "REPOSITORY", s);

                // init our SubscriptionRepository
                subscriptionRepository.PreWarm(CancellationToken.None).Wait();
                
                List<SubscriptionWorker> workers = ConfigureSubscriptions(subscriptionRepository, subscriptionWorkersRoot);
                foreach (SubscriptionWorker subscriptionWorker in workers) {
                    subscriptionWorker.StartAsync(CancellationToken.None).Wait();
                }
            }

            if (Startup.AutoApiLogonOperators.Any()) {
                foreach (String autoApiLogonOperator in Startup.AutoApiLogonOperators) {
                    OperatorLogon(autoApiLogonOperator);
                }
            }
        }

        private static List<SubscriptionWorker> ConfigureSubscriptions(ISubscriptionRepository subscriptionRepository, SubscriptionWorkersRoot configuration) {
            List<SubscriptionWorker> workers = new List<SubscriptionWorker>();

            foreach (SubscriptionWorkerConfig configurationSubscriptionWorker in configuration.SubscriptionWorkers) {
                if (configurationSubscriptionWorker.Enabled == false)
                    continue;

                if (configurationSubscriptionWorker.IsOrdered) {
                    IDomainEventHandlerResolver eventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Ordered");
                    SubscriptionWorker worker = SubscriptionWorker.CreateOrderedSubscriptionWorker(Startup.EventStoreClientSettings,
                                                                                                   eventHandlerResolver,
                                                                                                   subscriptionRepository,
                                                                                                   configuration.PersistentSubscriptionPollingInSeconds);
                    worker.Trace += (_,
                                     args) => Extensions.orderedLog(TraceEventType.Information, args.Message);
                    worker.Warning += (_,
                                       args) => Extensions.orderedLog(TraceEventType.Warning, args.Message);
                    worker.Error += (_,
                                     args) => Extensions.orderedLog(TraceEventType.Error, args.Message);
                    worker.SetIgnoreGroups(configurationSubscriptionWorker.IgnoreGroups);
                    worker.SetIgnoreStreams(configurationSubscriptionWorker.IgnoreStreams);
                    worker.SetIncludeGroups(configurationSubscriptionWorker.IncludeGroups);
                    worker.SetIncludeStreams(configurationSubscriptionWorker.IncludeStreams);
                    workers.Add(worker);
                    
                }
                else {
                    for (Int32 i = 0; i < configurationSubscriptionWorker.InstanceCount; i++) {
                        IDomainEventHandlerResolver eventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Main");
                        SubscriptionWorker worker = SubscriptionWorker.CreateSubscriptionWorker(Startup.EventStoreClientSettings,
                                                                                                eventHandlerResolver,
                                                                                                subscriptionRepository,
                                                                                                configurationSubscriptionWorker.InflightMessages,
                                                                                                configuration.PersistentSubscriptionPollingInSeconds);

                        worker.Trace += (_,
                                         args) => Extensions.mainLog(TraceEventType.Information, args.Message);
                        worker.Warning += (_,
                                           args) => Extensions.mainLog(TraceEventType.Warning, args.Message);
                        worker.Error += (_,
                                         args) => Extensions.mainLog(TraceEventType.Error, args.Message);

                        worker.SetIgnoreGroups(configurationSubscriptionWorker.IgnoreGroups);
                        worker.SetIgnoreStreams(configurationSubscriptionWorker.IgnoreStreams);
                        worker.SetIncludeGroups(configurationSubscriptionWorker.IncludeGroups);
                        worker.SetIncludeStreams(configurationSubscriptionWorker.IncludeStreams);

                        workers.Add(worker);
                    }
                }
            }
            
            return workers;
        }

        private static void OperatorLogon(String operatorId)
        {
            try {
                Logger.LogInformation($"About to do auto logon for operator Id [{operatorId}]");
                Func<String, IOperatorProxy> resolver = Startup.ServiceProvider.GetService<Func<String, IOperatorProxy>>();
                IOperatorProxy proxy = resolver(operatorId);

                OperatorResponse logonResult = proxy.ProcessLogonMessage(null, CancellationToken.None).Result;
                Logger.LogInformation($"Auto logon for operator Id [{operatorId}] status [{logonResult.IsSuccessful}]");
            }
            catch(Exception ex) {
                Logger.LogWarning($"Auto logon for operator Id [{operatorId}] failed.");
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class SubscriptionWorkersRoot
    {
        public Boolean InternalSubscriptionService { get; set; }
        public Int32 PersistentSubscriptionPollingInSeconds { get; set; }
        public Int32 InternalSubscriptionServiceCacheDuration { get; set; }
        public List<SubscriptionWorkerConfig> SubscriptionWorkers { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class SubscriptionWorkerConfig
    {
        public String WorkerName { get; set; }
        public String IncludeGroups { get; set; }
        public String IgnoreGroups { get; set; }
        public String IncludeStreams { get; set; }
        public String IgnoreStreams { get; set; }
        public Boolean Enabled { get; set; }
        public Int32 InflightMessages { get; set; }
        public Int32 InstanceCount { get; set; }
        public Boolean IsOrdered { get; set; }
    }
}