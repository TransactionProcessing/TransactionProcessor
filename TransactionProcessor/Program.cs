using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Sentry.Extensibility;
using Shared.General;
using Logger = Shared.Logger.Logger;
using NlogLogger = Shared.Logger.NlogLogger;
using Shared.Middleware;
using Shared.Serialisation;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TransactionProcessor.Bootstrapper;
using TransactionProcessor.Endpoints;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;
using Shared.EventStore.Aggregate;
using Shared.Extensions;
using TransactionProcessing.ApiDefaults;

namespace TransactionProcessor;

[ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program()
    {
    }

    public static void Main(string[] args)
    {
        BuildApplication(args).Run();
    }

    public static WebApplication BuildApplication(string[] args)
    {
        string contentRoot = Directory.GetCurrentDirectory();
        string nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

        LogManager.Setup(b =>
        {
            b.SetupLogFactory(setup =>
            {
                setup.AddCallSiteHiddenAssembly(typeof(NlogLogger).Assembly);
                setup.AddCallSiteHiddenAssembly(typeof(Shared.Logger.Logger).Assembly);
                setup.AddCallSiteHiddenAssembly(typeof(TenantMiddleware).Assembly);
            });
            b.LoadConfigurationFromFile(nlogConfigPath);
        });

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Host.UseWindowsService();
        builder.Host.UseLamar();

        ConfigureConfiguration(builder);
        ConfigureLogging(builder);

        IConfigurationRoot configuration = builder.Configuration;
        IWebHostEnvironment environment = builder.Environment;
        ConfigurationReader.Initialise(configuration);
        ConfigureSentry(builder, configuration, environment);

        builder.Host.ConfigureContainer<ServiceRegistry>((_, services) =>
        {
            ConfigureServices(services, configuration, environment);
        });

        WebApplication app = builder.Build();
        LogManager.Setup().LoadConfigurationFromFile("NLog.config");
        NLog.Logger logger = LogManager.GetLogger("TransactionProcessor");
        Shared.Logger.Logger.Initialise(logger, "TransactionProcessor");
        configuration.LogConfiguration(Logger.LogWarning);
        StringSerialiser.Initialise(app.Services.GetRequiredService<IStringSerialiser>());
        TypeProvider.LoadDomainEventsTypeDynamically();
        LogTypeMappings();

        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseMiddleware<TenantMiddleware>();
        app.AddRequestResponseLogging();
        app.AddExceptionHandler();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMetricServer();
        ConfigureEndpoints(app);
        app.PreWarm();
        TraceLoadedTokenAssemblies();
        app.UseApiDefaults();
        return app;
    }

    public static void ConfigureServices(ServiceRegistry services, IConfigurationRoot configuration, IWebHostEnvironment environment)
    {
        ConfigurationReader.Initialise(configuration);
        services.IncludeRegistry<MiscRegistry>();
        services.IncludeRegistry<MediatorRegistry>();
        services.IncludeRegistry(new RepositoryRegistry(configuration, environment));
        services.IncludeRegistry(new MiddlewareRegistry(configuration));
        services.IncludeRegistry<DomainServiceRegistry>();
        services.IncludeRegistry(new OperatorRegistry(configuration));
        services.IncludeRegistry<ClientRegistry>();
        services.IncludeRegistry<SerialiserRegistry>();
        services.IncludeRegistry(new DomainEventHandlerRegistry(configuration));
        services.AddMemoryCache();
        services.AddHostedService<AutoLogonWorkerService>();
    }

    private static void ConfigureConfiguration(WebApplicationBuilder builder)
    {
        FileInfo assemblyFile = new(Assembly.GetExecutingAssembly().Location);
        builder.Configuration.SetBasePath(assemblyFile.Directory!.FullName)
            .AddJsonFile("hosting.json", optional: true)
            .AddJsonFile($"hosting.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddJsonFile("/home/txnproc/config/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"/home/txnproc/config/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("/home/txnproc/config/appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthMonitoring:Service:Version"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0"
            });
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Logging.AddConsole();
        builder.Logging.AddNLog();
    }

    private static void ConfigureSentry(WebApplicationBuilder builder, IConfiguration configuration, IWebHostEnvironment environment)
    {
        IConfigurationSection sentrySection = configuration.GetSection("SentryConfiguration");
        if (sentrySection.Exists() && !environment.IsDevelopment())
        {
            builder.WebHost.UseSentry(options =>
            {
                options.Dsn = configuration["SentryConfiguration:Dsn"];
                options.SendDefaultPii = true;
                options.MaxRequestBodySize = RequestSize.Always;
                options.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
                options.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
                options.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            });
        }
    }

    private static void ConfigureEndpoints(WebApplication app)
    {
        app.MapDomainEventEndpoints();
        app.MapHealthChecks("health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
        });
        app.MapHealthChecks("healthui", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapEstateEndpoints();
        app.MapMerchantEndpoints();
        app.MapContractEndpoints();
        app.MapOperatorEndpoints();
        app.MapFloatEndpoints();
        app.MapVoucherEndpoints();
        app.MapTransactionEndpoints();
        app.MapSettlementEndpoints();

    }

    private static void LogTypeMappings()
    {
        foreach (KeyValuePair<Type, string> type in TypeMap.Map)
        {
            Logger.LogInformation($"Type name {type.Value} mapped to {type.Key.Name}");
        }
    }

    private static void TraceLoadedTokenAssemblies()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == "Microsoft.IdentityModel.Tokens"))
        {
            Console.WriteLine($"[Trace] Tokens Assembly: {assembly.Location} - Version: {assembly.GetName().Version}");
        }
    }
}
