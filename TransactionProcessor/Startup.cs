using Prometheus;
using System.Linq;
using System.Threading;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Endpoints;

namespace TransactionProcessor
{
    using Bootstrapper;
    using HealthChecks.UI.Client;
    using Lamar;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NLog;
    using NLog.Extensions.Logging;
    using Shared.EventStore.Aggregate;
    using Shared.Extensions;
    using Shared.General;
    using Shared.Middleware;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using ILogger = Microsoft.Extensions.Logging.ILogger;
    using Logger = Shared.Logger.Logger;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        #region Fields

        public static Container Container;

        private static readonly List<String> AutoApiLogonOperatorsInternal = new List<String>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="webHostEnvironment">The web host environment.</param>
        public Startup(IWebHostEnvironment webHostEnvironment) {
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

        public static IReadOnlyCollection<String> AutoApiLogonOperators => AutoApiLogonOperatorsInternal.AsReadOnly();

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
            ConfigurationReader.Initialise(Startup.Configuration);
            
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            
            ILogger logger = loggerFactory.CreateLogger("TransactionProcessor");

            Logger.Initialise(logger);

            Startup.Configuration.LogConfiguration(Logger.LogWarning);
            this.LogTypeMappings();
            app.UseMiddleware<TenantMiddleware>();
            app.AddRequestResponseLogging();
            app.AddExceptionHandler();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMetricServer(s => {
            });
            this.ConfigureEndpoints(app);

            app.UseSwagger();

            app.UseSwaggerUI();

            app.PreWarm();
            this.TraceLoadedTokenAssemblies();
        }

        public void ConfigureContainer(ServiceRegistry services) {

            ConfigurationReader.Initialise(Startup.Configuration);
            Startup.AutoApiLogonOperatorsInternal.Clear();

            services.IncludeRegistry<MiscRegistry>();
            services.IncludeRegistry<MediatorRegistry>();
            services.IncludeRegistry<RepositoryRegistry>();
            services.IncludeRegistry<MiddlewareRegistry>();
            services.IncludeRegistry<DomainServiceRegistry>();
            services.IncludeRegistry<OperatorRegistry>();
            services.IncludeRegistry<ClientRegistry>();
            services.IncludeRegistry<DomainEventHandlerRegistry>();
            
            services.AddMemoryCache();

            TypeProvider.LoadDomainEventsTypeDynamically();

            Startup.Container = new Container(services);

            Startup.ServiceProvider = services.BuildServiceProvider();
        }

        internal static void AddAutoApiLogonOperator(String operatorId) {
            if (Startup.AutoApiLogonOperatorsInternal.Contains(operatorId) == false) {
                Startup.AutoApiLogonOperatorsInternal.Add(operatorId);
            }
        }

        private void ConfigureEndpoints(IApplicationBuilder app) {
            app.UseEndpoints(endpoints => {
                                 endpoints.MapControllers();
                                 endpoints.MapHealthChecks("health",
                                                           new HealthCheckOptions {
                                                                                      Predicate = _ => true,
                                                                                      ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
                                                                                  });
                                 endpoints.MapHealthChecks("healthui",
                                                           new HealthCheckOptions {
                                                                                      Predicate = _ => true,
                                                                                      ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                                                                                  });
                                 endpoints.MapEstateEndpoints();
                                 endpoints.MapMerchantEndpoints();
                                 endpoints.MapContractEndpoints();
                                 endpoints.MapOperatorEndpoints();
                                 endpoints.MapFloatEndpoints();
                                 endpoints.MapVoucherEndpoints();
                                 endpoints.MapTransactionEndpoints();
                                 endpoints.MapSettlementEndpoints();
                             });
        }

        private void LogTypeMappings() {
            foreach (KeyValuePair<Type, String> type in TypeMap.Map) {
                Logger.LogInformation($"Type name {type.Value} mapped to {type.Key.Name}");
            }
        }

        private void TraceLoadedTokenAssemblies() {
            List<Assembly> loadedTokensAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                                                          .Where(a => a.GetName().Name == "Microsoft.IdentityModel.Tokens")
                                                          .ToList();

            foreach (Assembly asm in loadedTokensAssemblies) {
                Console.WriteLine($"[Trace] Tokens Assembly: {asm.Location} - Version: {asm.GetName().Version}");
            }
        }
    }

    #endregion
}
