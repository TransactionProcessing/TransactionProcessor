using Shouldly;

namespace TransactionProcessor.Tests.General
{
    using Lamar;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Imposter.Abstractions;
    using Shared.Serialisation;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using TransactionProcessor.ProjectionEngine.State;
    using Xunit;

    /// <summary>
    /// 
    /// </summary>
    public class BootstrapperTests
    {
        #region Methods

        [Fact]
        public void ConfigureContainer_PopulatesAutoApiLogonOperators_WithoutDuplicates()
        {
            IWebHostEnvironmentImposter hostingEnvironment = new IWebHostEnvironmentImposter();
            hostingEnvironment.EnvironmentName.Getter().Returns("Development");
            hostingEnvironment.ContentRootPath.Getter().Returns("/home");
            hostingEnvironment.ApplicationName.Getter().Returns("Test Application");

            Startup s = new Startup(hostingEnvironment.Instance());
            Startup.Configuration = this.SetupMemoryConfiguration();

            ServiceRegistry firstServices = new ServiceRegistry();
            this.AddTestRegistrations(firstServices, hostingEnvironment.Instance());
            s.ConfigureContainer(firstServices);

            Assert.Equal(new[] { "Safaricom", "PataPawaPostPay" }, Startup.AutoApiLogonOperators.ToArray());

            ServiceRegistry secondServices = new ServiceRegistry();
            this.AddTestRegistrations(secondServices, hostingEnvironment.Instance());
            s.ConfigureContainer(secondServices);

            Assert.Equal(new[] { "Safaricom", "PataPawaPostPay" }, Startup.AutoApiLogonOperators.ToArray());
        }

        private IConfigurationRoot SetupMemoryConfiguration()
        {
            Dictionary<String, String> configuration = new Dictionary<String, String>();

            IConfigurationBuilder builder = new ConfigurationBuilder();

            configuration.Add("EventStoreSettings:ConnectionString", "esdb://127.0.0.1:2113");
            configuration.Add("EventStoreSettings:ConnectionName", "UnitTestConnection");
            configuration.Add("AppSettings:UseConnectionStringConfig", "false");
            configuration.Add("AppSettings:ClientId", "clientId");
            configuration.Add("AppSettings:ClientSecret", "clientSecret");
            configuration.Add("AppSettings:EstateManagementApi", "http://localhost");
            configuration.Add("AppSettings:VoucherManagementApi", "http://localhost");
            configuration.Add("AppSettings:SecurityService", "http://localhost");
            configuration.Add("SecurityConfiguration:Authority", "http://localhost");
            configuration.Add("SecurityConfiguration:ApiName", "ApiName");
            configuration.Add("ConnectionStrings:TransactionProcessorReadModel", "dbconnstring");
            configuration.Add("OperatorConfiguration:Safaricom:ApiLogonRequired", "true");
            configuration.Add("OperatorConfiguration:PataPawaPostPay:ApiLogonRequired", "true");
            configuration.Add("OperatorConfiguration:PataPawaPrePay:ApiLogonRequired", "false");

            builder.AddInMemoryCollection(configuration);

            return builder.Build();
        }

        /// <summary>
        /// Adds the test registrations.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="hostingEnvironment">The hosting environment.</param>
        private void AddTestRegistrations(IServiceCollection services,
                                          IWebHostEnvironment hostingEnvironment)
        {
            services.AddLogging();
            DiagnosticListener diagnosticSource = new DiagnosticListener(hostingEnvironment.ApplicationName);
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton(diagnosticSource);
            services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
            services.AddSingleton<IConfiguration>(Startup.Configuration);
        }

        #endregion
    }
}
