using SimpleResults;

namespace TransactionProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Responses.Contract;
    using EventStore.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using SecurityService.Client;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.EventStore.EventStore;
    using Shared.EventStore.Extensions;
    using Shared.EventStore.SubscriptionWorker;
    using Shared.General;
    using Shared.Logger;
    using TransactionProcessor.BusinessLogic.OperatorInterfaces;

    [ExcludeFromCodeCoverage]
    public static class Extensions {
        static Action<TraceEventType, String, String> log = (tt,
                                                             subType,
                                                             message) => {
            String logMessage = $"{subType} - {message}";
            switch (tt) {
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

        static Action<TraceEventType, String> mainLog = (tt,
                                                         message) => Extensions.log(tt, "MAIN", message);

        static Action<TraceEventType, String> orderedLog = (tt,
                                                            message) => Extensions.log(tt, "ORDERED", message);

        public static void PreWarm(this IApplicationBuilder applicationBuilder) {
            TypeProvider.LoadDomainEventsTypeDynamically();

            IConfigurationSection subscriptionConfigSection = Startup.Configuration.GetSection("AppSettings:SubscriptionConfiguration");
            SubscriptionWorkersRoot subscriptionWorkersRoot = new SubscriptionWorkersRoot();
            subscriptionConfigSection.Bind(subscriptionWorkersRoot);

            String eventStoreConnectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");

            IDomainEventHandlerResolver mainEventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Main");
            IDomainEventHandlerResolver orderedEventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Ordered");

            Dictionary<String, IDomainEventHandlerResolver> eventHandlerResolvers = new Dictionary<String, IDomainEventHandlerResolver> { { "Main", mainEventHandlerResolver }, { "Ordered", orderedEventHandlerResolver } };


            Func<String, Int32, ISubscriptionRepository> subscriptionRepositoryResolver = Startup.Container.GetInstance<Func<String, Int32, ISubscriptionRepository>>();

            String connectionString = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString");
            EventStoreClientSettings eventStoreClientSettings = EventStoreClientSettings.Create(connectionString);

            applicationBuilder.ConfigureSubscriptionService(subscriptionWorkersRoot, eventStoreConnectionString, eventHandlerResolvers, Extensions.log, subscriptionRepositoryResolver).Wait(CancellationToken.None);
        }

    }

    [ExcludeFromCodeCoverage]
    public class AutoLogonWorkerService : BackgroundService {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

            while (stoppingToken.IsCancellationRequested == false) {

                if (Startup.AutoApiLogonOperators.Any()) {
                    foreach (String autoApiLogonOperator in Startup.AutoApiLogonOperators) {
                        OperatorLogon(autoApiLogonOperator);
                    }
                }

                String fileProfilePollingWindowInSeconds = "5";
                // Delay for configured seconds before polling for files again
                await Task.Delay(TimeSpan.FromSeconds(int.Parse(fileProfilePollingWindowInSeconds)), stoppingToken);
            }
        }

        private static void OperatorLogon(String operatorId) {
            try {
                Logger.LogInformation($"About to do auto logon for operator Id [{operatorId}]");
                Func<String, IOperatorProxy> resolver = Startup.ServiceProvider.GetService<Func<String, IOperatorProxy>>();
                IOperatorProxy proxy = resolver(operatorId);

                Result<OperatorResponse> logonResult = proxy.ProcessLogonMessage(CancellationToken.None).Result;

                if (logonResult.IsSuccess) {
                    Logger.LogInformation($"Auto logon for operator Id [{operatorId}] status [{logonResult.Data.IsSuccessful}]");
                }

                if (logonResult.IsFailed) {
                    Logger.LogWarning($"Auto logon for operator Id [{operatorId}] status [{logonResult.Message}]");
                }
            }
            catch (Exception ex) {
                Logger.LogWarning($"Auto logon for operator Id [{operatorId}] failed.");
                Logger.LogWarning(ex.ToString());
            }
        }
    }
}