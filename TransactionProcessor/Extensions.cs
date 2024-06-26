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
    public static class Extensions
    {
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
            TypeProvider.LoadDomainEventsTypeDynamically();

            IConfigurationSection subscriptionConfigSection = Startup.Configuration.GetSection("AppSettings:SubscriptionConfiguration");
            SubscriptionWorkersRoot subscriptionWorkersRoot = new SubscriptionWorkersRoot();
            subscriptionConfigSection.Bind(subscriptionWorkersRoot);

            String eventStoreConnectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");

            IDomainEventHandlerResolver mainEventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Main");
            IDomainEventHandlerResolver orderedEventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Ordered");

            Dictionary<String, IDomainEventHandlerResolver> eventHandlerResolvers = new Dictionary<String, IDomainEventHandlerResolver> {
                                                                                        {"Main", mainEventHandlerResolver},
                                                                                        {"Ordered", orderedEventHandlerResolver}
                                                                                    };


            Func<String, Int32, ISubscriptionRepository> subscriptionRepositoryResolver = Startup.Container.GetInstance<Func<String, Int32, ISubscriptionRepository>>();

            String connectionString = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString");
            EventStoreClientSettings eventStoreClientSettings = EventStoreClientSettings.Create(connectionString);

            applicationBuilder.ConfigureSubscriptionService(subscriptionWorkersRoot,
                                                            eventStoreConnectionString,
                                                            eventHandlerResolvers,
                                                            Extensions.log,
                                                            subscriptionRepositoryResolver).Wait(CancellationToken.None);

            //LoadContractData(CancellationToken.None).Wait(CancellationToken.None);
            
        }

        private static async Task LoadContractData(CancellationToken cancellationToken){
            IEstateClient estateClient = Startup.Container.GetRequiredService<IEstateClient>();
            ISecurityServiceClient securityServiceClient = Startup.Container.GetRequiredService<ISecurityServiceClient>();
            IEventStoreContext eventStoreContext = Startup.Container.GetRequiredService<IEventStoreContext>();

            var clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
            var clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
            var token = await securityServiceClient.GetToken(clientId, clientSecret, cancellationToken);

            List<ContractResponse> contractResponses = new List<ContractResponse>();

            Stopwatch sw = Stopwatch.StartNew();
            // get a list of the contracts from ES projection
            String state = await eventStoreContext.GetStateFromProjection("ContractList",cancellationToken);
            ContractList contractList = JsonConvert.DeserializeObject<ContractList>(state);
            contractList.Contracts.AddRange(contractList.Contracts);
            contractList.Contracts.AddRange(contractList.Contracts);
            contractList.Contracts.AddRange(contractList.Contracts);
            contractList.Contracts.AddRange(contractList.Contracts);
            contractList.Contracts.AddRange(contractList.Contracts);
            contractList.Contracts.AddRange(contractList.Contracts);

            List<Task<ContractResponse>> tasks = new ();
            foreach (var contract in contractList.Contracts){
                //ContractResponse contractResponse = await estateClient.GetContract(token.AccessToken, contract.EstateId, contract.ContractId, cancellationToken);
                //contractResponses.Add(contractResponse);
                tasks.Add(estateClient.GetContract(token.AccessToken, contract.EstateId, contract.ContractId, cancellationToken));
            }

            ContractResponse[] t = await Task.WhenAll(tasks);

            contractResponses = t.ToList();
            if (contractResponses.Any()){
                IMemoryCache memoryCache = Startup.Container.GetRequiredService<IMemoryCache>();
                //    // Build up the cache
                Dictionary<(Guid, Guid), List<ContractProductTransactionFee>> productfees = contractResponses
                                                                                            .SelectMany(contractResponse => contractResponse.Products.Select(contractResponseProduct =>
                                                                                                                                                                 new{
                                                                                                                                                                        //Key = (contractResponse.EstateId, contractResponseProduct.ProductId),
                                                                                                                                                                        Key = (Guid.NewGuid(),Guid.NewGuid()),
                                                                                                                                                                        Value = contractResponseProduct.TransactionFees
                                                                                                                                                                    }))
                                                                                            .ToDictionary(x => x.Key, x => x.Value);
                memoryCache.Set("TransactionFeeCache", productfees);
            }
            sw.Stop();
            Logger.LogWarning($"Contract Data loaded an cached [{sw.ElapsedMilliseconds} ms");
        }
    }

    public class AutoLogonWorkerService : BackgroundService{
        protected override async Task ExecuteAsync(CancellationToken stoppingToken){

            while (stoppingToken.IsCancellationRequested == false){
                
                if (Startup.AutoApiLogonOperators.Any()){
                    foreach (String autoApiLogonOperator in Startup.AutoApiLogonOperators){
                        OperatorLogon(autoApiLogonOperator);
                    }
                }

                String fileProfilePollingWindowInSeconds ="5";
                // Delay for configured seconds before polling for files again
                await Task.Delay(TimeSpan.FromSeconds(int.Parse(fileProfilePollingWindowInSeconds)), stoppingToken);
            }
        }

        private static void OperatorLogon(String operatorId)
        {
            try
            {
                Logger.LogInformation($"About to do auto logon for operator Id [{operatorId}]");
                Func<String, IOperatorProxy> resolver = Startup.ServiceProvider.GetService<Func<String, IOperatorProxy>>();
                IOperatorProxy proxy = resolver(operatorId);

                OperatorResponse logonResult = proxy.ProcessLogonMessage(null, CancellationToken.None).Result;
                Logger.LogInformation($"Auto logon for operator Id [{operatorId}] status [{logonResult.IsSuccessful}]");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Auto logon for operator Id [{operatorId}] failed.");
                Logger.LogWarning(ex.ToString());
            }
        }
    }

    public class Contract
    {
        public Guid EstateId { get; set; }
        public Guid ContractId { get; set; }
    }

    public class ContractList
    {
        public List<Contract> Contracts { get; set; }
    }
}