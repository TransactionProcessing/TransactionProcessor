using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Shared.Logger;
using Shared.Middleware;
using System;

namespace TransactionProcessor
{
    using Lamar.Microsoft.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using NLog;
    using Sentry.Extensibility;
    using Shared.General;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            Program.CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            String contentRoot = Directory.GetCurrentDirectory();
            String nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

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

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseWindowsService();
            hostBuilder.UseLamar();
            ConfigureWebHost(hostBuilder);
            hostBuilder.ConfigureLogging(logging => {
                logging.AddConsole();
                logging.AddNLog();

            });
            hostBuilder.ConfigureServices(services =>
                                          {
                                              services.AddHostedService<AutoLogonWorkerService>(provider =>
                                                                                                    {
                                                                                                        AutoLogonWorkerService worker = new AutoLogonWorkerService();
                                                                                                        return worker;
                                                                                                    });
                                          });
            return hostBuilder;
        }

        private static void ConfigureWebHost(IHostBuilder hostBuilder) {
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureAppConfiguration((context, configBuilder) =>
                {
                    IWebHostEnvironment env = context.HostingEnvironment;

                    FileInfo fi = new(Assembly.GetExecutingAssembly().Location);

                    configBuilder.SetBasePath(fi.Directory.FullName)
                        .AddJsonFile("hosting.json", optional: true)
                        .AddJsonFile($"hosting.{env.EnvironmentName}.json", optional: true)
                        .AddJsonFile("/home/txnproc/config/appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"/home/txnproc/config/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();

                    // Build a snapshot of configuration so we can use it immediately (e.g. for Sentry)
                    IConfigurationRoot builtConfig = configBuilder.Build();

                    // Keep existing static usage (if you must), and initialise the ConfigurationReader now.
                    Startup.Configuration = builtConfig;
                    ConfigurationReader.Initialise(Startup.Configuration);

                    // Configure Sentry on the webBuilder using the config snapshot.
                    IConfigurationSection sentrySection = builtConfig.GetSection("SentryConfiguration");
                    if (sentrySection.Exists())
                    {
                        // Replace the condition below if you intended to only enable Sentry in certain environments.
                        if (env.IsDevelopment() == false)
                        {
                            webBuilder.UseSentry(o =>
                            {
                                o.Dsn = builtConfig["SentryConfiguration:Dsn"];
                                o.SendDefaultPii = true;
                                o.MaxRequestBodySize = RequestSize.Always;
                                o.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
                                o.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
                                o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
                            });
                        }
                    }
                });

                webBuilder.UseStartup<Startup>();
                webBuilder.UseKestrel();
            });
        }
    }
}
