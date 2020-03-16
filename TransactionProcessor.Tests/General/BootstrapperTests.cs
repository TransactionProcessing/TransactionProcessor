namespace TransactionProcessor.Tests.General
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Xunit;

    /// <summary>
    /// 
    /// </summary>
    public class BootstrapperTests
    {
        #region Methods

        /// <summary>
        /// Verifies the bootstrapper is valid.
        /// </summary>
        [Fact]
        public void VerifyBootstrapperIsValid()
        {
            Mock<IWebHostEnvironment> hostingEnvironment = new Mock<IWebHostEnvironment>();
            hostingEnvironment.Setup(he => he.EnvironmentName).Returns("Development");
            hostingEnvironment.Setup(he => he.ContentRootPath).Returns("/home");
            hostingEnvironment.Setup(he => he.ApplicationName).Returns("Test Application");

            IServiceCollection services = new ServiceCollection();
            Startup s = new Startup(hostingEnvironment.Object);
            Startup.Configuration = this.SetupMemoryConfiguration();

            s.ConfigureServices(services);

            this.AddTestRegistrations(services, hostingEnvironment.Object);

            services.AssertConfigurationIsValid();
        }

        private IConfigurationRoot SetupMemoryConfiguration()
        {
            Dictionary<String, String> configuration = new Dictionary<String, String>();

            IConfigurationBuilder builder = new ConfigurationBuilder();

            configuration.Add("EventStoreSettings:ConnectionString", "ConnectTo=tcp://admin:changeit@127.0.0.1:1112;VerboseLogging=true;");
            configuration.Add("EventStoreSettings:ConnectionName", "UnitTestConnection");
            configuration.Add("EventStoreSettings:HttpPort", "2113");
            configuration.Add("AppSettings:UseConnectionStringConfig", "false");
            configuration.Add("AppSettings:ClientId", "clientId");
            configuration.Add("AppSettings:ClientSecret", "clientSecret");
            configuration.Add("AppSettings:EstateManagementApi", "http://localhost");
            configuration.Add("AppSettings:SecurityService", "http://localhost");

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
            services.AddSingleton(hostingEnvironment);
        }

        #endregion
    }

    public static class ServiceCollectionExtensions
    {
        public static void AssertConfigurationIsValid(this IServiceCollection serviceCollection,
                                                      List<Type> typesToIgnore = null)
        {
            ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider();

            List<ServiceDescriptor> list = serviceCollection.Where(x => x.ServiceType.Namespace != null && x.ServiceType.Namespace.Contains("Vme")).ToList();

            if (typesToIgnore != null)
            {
                list.RemoveAll(listItem => typesToIgnore.Contains(listItem.ServiceType));
            }

            foreach (ServiceDescriptor serviceDescriptor in list)
            {
                Type type = serviceDescriptor.ServiceType;

                //This throws an Exception if the type cannot be instantiated.
                buildServiceProvider.GetService(type);
            }
        }
    }
}