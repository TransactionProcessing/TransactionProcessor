namespace TransactionProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading;
    using Bootstrapper;
    using EstateManagement.Estate.DomainEvents;
    using EstateManagement.Merchant.DomainEvents;
    using EventStore.Client;
    using FileProcessor.File.DomainEvents;
    using FileProcessor.FileImportLog.DomainEvents;
    using HealthChecks.UI.Client;
    using Lamar;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using NuGet.Protocol;
    using ProjectionEngine.EventHandling;
    using ProjectionEngine.State;
    using Reconciliation.DomainEvents;
    using Settlement.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Logger;
    using Transaction.DomainEvents;
    using TransactionProcessor.BusinessLogic.OperatorInterfaces;
    using Voucher.DomainEvents;
    using EventHandler = ProjectionEngine.EventHandling.EventHandler;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        #region Fields

        public static Container Container;

        /// <summary>
        /// The event store client settings
        /// </summary>
        internal static EventStoreClientSettings EventStoreClientSettings;

        public static List<String> AutoApiLogonOperators = new List<String>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="webHostEnvironment">The web host environment.</param>
        public Startup(IWebHostEnvironment webHostEnvironment) {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(webHostEnvironment.ContentRootPath)
                                                                      .AddJsonFile("/home/txnproc/config/appsettings.json", true, true)
                                                                      .AddJsonFile($"/home/txnproc/config/appsettings.{webHostEnvironment.EnvironmentName}.json",
                                                                                   optional:true).AddJsonFile("appsettings.json", optional:true, reloadOnChange:true)
                                                                      .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json",
                                                                                   optional:true,
                                                                                   reloadOnChange:true).AddEnvironmentVariables();

            Startup.Configuration = builder.Build();
            Startup.WebHostEnvironment = webHostEnvironment;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public static IConfigurationRoot Configuration { get; set; }

        public static IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Gets or sets the web host environment.
        /// </summary>
        /// <value>
        /// The web host environment.
        /// </value>
        public static IWebHostEnvironment WebHostEnvironment { get; set; }

        #endregion

        #region Methods

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="provider">The provider.</param>
        public void Configure(IApplicationBuilder app,
                              IWebHostEnvironment env,
                              ILoggerFactory loggerFactory) {
            String nlogConfigFilename = "nlog.config";

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.ConfigureNLog(Path.Combine(env.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("TransactionProcessor");

            Logger.Initialise(logger);

            Action<String> loggerAction = message => { Logger.LogInformation(message); };
            Startup.Configuration.LogConfiguration(loggerAction);

            foreach (KeyValuePair<Type, String> type in TypeMap.Map) {
                Logger.LogInformation($"Type name {type.Value} mapped to {type.Key.Name}");
            }

            app.AddRequestLogging();
            app.AddResponseLogging();
            app.AddExceptionHandler();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                                 endpoints.MapControllers();
                                 endpoints.MapHealthChecks("health",
                                                           new HealthCheckOptions {
                                                                                      Predicate = _ => true,
                                                                                      ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
                                                                                  });
                             });

            app.UseSwagger();

            app.UseSwaggerUI();

            app.PreWarm();
            
        }

        public void ConfigureContainer(ServiceRegistry services) {

            ConfigurationReader.Initialise(Startup.Configuration);

            services.IncludeRegistry<MediatorRegistry>();
            services.IncludeRegistry<RepositoryRegistry>();
            services.IncludeRegistry<MiddlewareRegistry>();
            services.IncludeRegistry<DomainServiceRegistry>();
            services.IncludeRegistry<OperatorRegistry>();
            services.IncludeRegistry<ClientRegistry>();
            services.IncludeRegistry<DomainEventHandlerRegistry>();
            services.IncludeRegistry<MiscRegistry>();
            services.AddMemoryCache();

            Startup.LoadTypes();

            Startup.Container = new Container(services);

            Startup.ServiceProvider = services.BuildServiceProvider();
        }

        public static void LoadTypes() {
            SettlementCreatedForDateEvent s =
                new SettlementCreatedForDateEvent(Guid.Parse("62CA5BF0-D138-4A19-9970-A4F7D52DE292"), Guid.Parse("3E42516B-6C6F-4F86-BF08-3EF0ACDDDD55"), DateTime.Now);

            TransactionHasStartedEvent t = new TransactionHasStartedEvent(Guid.Parse("2AA2D43B-5E24-4327-8029-1135B20F35CE"),
                                                                          Guid.NewGuid(),
                                                                          Guid.NewGuid(),
                                                                          DateTime.Now,
                                                                          "",
                                                                          "",
                                                                          "",
                                                                          "",
                                                                          null);

            ReconciliationHasStartedEvent r = new ReconciliationHasStartedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

            EstateCreatedEvent c = new EstateCreatedEvent(Guid.NewGuid(), "");
            MerchantCreatedEvent m = new MerchantCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now);
            FileCreatedEvent f = new FileCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "", DateTime.Now);
            FileAddedToImportLogEvent fi = new FileAddedToImportLogEvent(Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         Guid.NewGuid(),
                                                                         "",
                                                                         "",
                                                                         DateTime.Now);
            VoucherGeneratedEvent g = new VoucherGeneratedEvent(Guid.NewGuid(),
                                                                Guid.NewGuid(),
                                                                Guid.NewGuid(),
                                                                DateTime.Now,
                                                                String.Empty,
                                                                0,
                                                                String.Empty,
                                                                DateTime.Now,
                                                                String.Empty);

            TypeProvider.LoadDomainEventsTypeDynamically();
        }

        /// <summary>
        /// Configures the event store settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        internal static void ConfigureEventStoreSettings(EventStoreClientSettings settings) {
            settings.ConnectivitySettings = EventStoreClientConnectivitySettings.Default;
            settings.ConnectivitySettings.Address = new Uri(Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString"));
            settings.ConnectivitySettings.Insecure = Startup.Configuration.GetValue<Boolean>("EventStoreSettings:Insecure");

            settings.DefaultCredentials = new UserCredentials(Startup.Configuration.GetValue<String>("EventStoreSettings:UserName"),
                                                              Startup.Configuration.GetValue<String>("EventStoreSettings:Password"));
            Startup.EventStoreClientSettings = settings;
        }
    }

    #endregion
}