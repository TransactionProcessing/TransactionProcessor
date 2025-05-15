using JasperFx.Core;
using MediatR;
using SimpleResults;
using System.IO;
using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Requests;

namespace TransactionProcessor
{
    using EventStore.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.EventStore.Extensions;
    using Shared.EventStore.SubscriptionWorker;
    using Shared.General;
    using Shared.Logger;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TransactionProcessor.BusinessLogic.OperatorInterfaces;
    using static TransactionProcessor.BusinessLogic.Requests.MerchantStatementCommands;

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
            IDomainEventHandlerResolver domainEventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Domain");
            IDomainEventHandlerResolver orderedEventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Ordered");

            Dictionary<String, IDomainEventHandlerResolver> eventHandlerResolvers = new Dictionary<String, IDomainEventHandlerResolver> { { "Main", mainEventHandlerResolver },
                { "Domain", domainEventHandlerResolver }, { "Ordered", orderedEventHandlerResolver } };


            Func<String, Int32, ISubscriptionRepository> subscriptionRepositoryResolver = Startup.Container.GetInstance<Func<String, Int32, ISubscriptionRepository>>();

            String connectionString = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString");
            EventStoreClientSettings eventStoreClientSettings = EventStoreClientSettings.Create(connectionString);

            applicationBuilder.ConfigureSubscriptionService(subscriptionWorkersRoot, eventStoreConnectionString, eventHandlerResolvers, Extensions.log, subscriptionRepositoryResolver).Wait(CancellationToken.None);

            // Setup the aggregate service caching
            IAggregateService aggregateService = Startup.ServiceProvider.GetService<IAggregateService>();
            aggregateService.AddCachedAggregate(typeof(EstateAggregate),null);
            aggregateService.AddCachedAggregate(typeof(ContractAggregate), null);
            aggregateService.AddCachedAggregate(typeof(OperatorAggregate), null);

            IMediator mediator = Startup.Container.GetInstance<IMediator>();
            StatementFilePollerService statementFilePollerService = new(mediator);
            statementFilePollerService.Start();
        }

    }

    [ExcludeFromCodeCoverage]
    public record FileProfile(String Name, String ListeningDirectory, String Filter, Boolean IsEnabled);

    [ExcludeFromCodeCoverage]
    public class StatementFilePollerService {
        private readonly IMediator Mediator;
        private readonly IEnumerable<FileProfile> FileProfiles;
        public StatementFilePollerService(IMediator mediator) {
            this.Mediator = mediator;
            // TODO: File Profiles
            // Load up the file configuration
            this.FileProfiles = Startup.Configuration.GetSection("AppSettings:FileProfiles").GetChildren().ToList().Select(x => new
            {
                Name = x.GetValue<String>("Name"),
                ListeningDirectory = x.GetValue<String>("ListeningDirectory"),
                Filter = x.GetValue<String>("Filter"),
                IsEnabled = x.GetValue<Boolean>("IsEnabled"),
            }).Select(f => new FileProfile(f.Name, f.ListeningDirectory, f.Filter, f.IsEnabled));
        }
        
        public void Start() {
            foreach (FileProfile fileProfile in this.FileProfiles) {
                Logger.LogWarning($"File Profile: {fileProfile.Name} Listening on Folder: [{fileProfile.ListeningDirectory}] IsEnabled: {fileProfile.IsEnabled}");
                Directory.CreateDirectory(fileProfile.ListeningDirectory);

                FileSystemWatcher fileSystemWatcher = new FileSystemWatcher
                {
                    Path = fileProfile.ListeningDirectory,
                    Filter = fileProfile.Filter
                };
                fileSystemWatcher.EnableRaisingEvents = fileProfile.IsEnabled;
                fileSystemWatcher.Created += (sender,
                                                 e) => FileHandler(sender, e, fileProfile);
            }
            
        }

        private void FileHandler(object sender, FileSystemEventArgs e, FileProfile fileProfile)
        {
            // Fire-and-forget async work
            Task.Run(async () =>
            {
                String processedFolder = Path.Combine(fileProfile.ListeningDirectory, "processed");
                String failedFolder = Path.Combine(fileProfile.ListeningDirectory, "failed");

                try
                {
                    Logger.LogInformation($"File detected on profile {fileProfile.Name} File name [{e.Name}]");

                    // Make sure the file is not locked by another process
                    await using (FileStream stream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        // Wait for the file to be released
                        await Task.Delay(1000);
                    }

                    // make sure the processed and failed folders are created
                    Directory.CreateDirectory(processedFolder);
                    Directory.CreateDirectory(failedFolder);

                    // Get the statement id from the file name
                    String fileName = Path.GetFileNameWithoutExtension(e.FullPath);
                    String[] parts = fileName.Split("_");
                    if (Guid.TryParse(parts[0], out Guid estateId) == false) {
                        throw new ArgumentException("Invalid estate ID in file name", nameof(e.FullPath));
                    }
                    if (Guid.TryParse(parts[1], out Guid statementId) == false)
                    {
                        throw new ArgumentException("Invalid statement ID in file name", nameof(e.FullPath));
                    }

                    String pdfData = await File.ReadAllTextAsync(e.FullPath, CancellationToken.None);

                    if (String.IsNullOrEmpty(pdfData)) {
                        throw new InvalidDataException("PDF Data is null or Empty");
                    }
                    // Create the email statement command
                    EmailMerchantStatementCommand command = new(estateId, statementId, pdfData);
                    
                    Result result = await this.Mediator.Send(command, CancellationToken.None);
                    if (result.IsFailed)
                    {
                        // Move the file to the failed folder
                        String failedFilePath = Path.Combine(failedFolder, e.Name);
                        File.Move(e.FullPath, failedFilePath);
                        // Log the error
                        Logger.LogWarning($"Failed to process file [{e.FullPath}]: {result.Message}");
                    }
                    else
                    {
                        // Move the file to the processed folder
                        String processedFilePath = Path.Combine(processedFolder, e.Name);
                        File.Move(e.FullPath, processedFilePath);
                        // Log success
                        Logger.LogInformation($"Successfully processed file [{e.FullPath}]");
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exception
                    // Move the file to the failed folder
                    String failedFilePath = Path.Combine(failedFolder, e.Name);
                    File.Move(e.FullPath, failedFilePath);
                    // Log the error
                    Console.Error.WriteLine($"Failed to process file [{e.FullPath}]: {ex.Message}");
                    Logger.LogError($"Failed to process file [{e.FullPath}]",ex);
                }
            }); 
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