using Prometheus;

namespace TransactionProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
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
            ConfigurationReader.Initialise(Startup.Configuration);

            String nlogConfigFilename = "nlog.config";

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                string directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                LogManager.AddHiddenAssembly(Assembly.LoadFrom(Path.Combine(directoryPath, "Shared.dll")));

                var developmentNlogConfigFilename = "nlog.development.config";
                if (File.Exists(Path.Combine(env.ContentRootPath, developmentNlogConfigFilename)))
                {
                    nlogConfigFilename = developmentNlogConfigFilename;
                }
            }
            else{
                LogManager.AddHiddenAssembly(Assembly.LoadFrom(Path.Combine(env.ContentRootPath, "Shared.dll")));
            }
            
            loggerFactory.ConfigureNLog(Path.Combine(env.ContentRootPath, nlogConfigFilename));
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger("TransactionProcessor");

            Logger.Initialise(logger);

            Startup.Configuration.LogConfiguration(Logger.LogWarning);

            foreach (KeyValuePair<Type, String> type in TypeMap.Map) {
                Logger.LogInformation($"Type name {type.Value} mapped to {type.Key.Name}");
            }

            app.AddRequestLogging();
            app.AddResponseLogging();
            app.AddExceptionHandler();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMetricServer(s => {
            });
            app.UseEndpoints(endpoints => {
                                 endpoints.MapControllers();
                                 endpoints.MapHealthChecks("health",
                                                           new HealthCheckOptions {
                                                                                      Predicate = _ => true,
                                                                                      ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
                                                                                  });
                                 endpoints.MapHealthChecks("healthui",
                                                           new HealthCheckOptions
                                                           {
                                                               Predicate = _ => true,
                                                               ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                                                           });
            });

            app.UseSwagger();

            app.UseSwaggerUI();

            app.PreWarm();
            
        }

        public void ConfigureContainer(ServiceRegistry services) {

            ConfigurationReader.Initialise(Startup.Configuration);

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
    }

    #endregion
}