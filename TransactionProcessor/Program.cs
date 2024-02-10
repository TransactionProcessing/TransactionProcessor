using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TransactionProcessor
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net.Http;
    using EventStore.Client;
    using Lamar.Microsoft.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Reconciliation.DomainEvents;
    using Settlement.DomainEvents;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.Logger;
    using Transaction.DomainEvents;

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            Program.CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //At this stage, we only need our hosting file for ip and ports
            FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(fi.Directory.FullName)
                                                                  .AddJsonFile("hosting.json", optional: true)
                                                                  .AddJsonFile("hosting.development.json", optional: true)
                                                                  .AddEnvironmentVariables().Build();

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseWindowsService();
            hostBuilder.UseLamar();
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                                                 {
                                                     webBuilder.UseStartup<Startup>();
                                                     webBuilder.UseConfiguration(config);
                                                     webBuilder.UseKestrel();
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
    }
}
