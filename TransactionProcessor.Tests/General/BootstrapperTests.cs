using Shouldly;

namespace TransactionProcessor.Tests.General
{
    using Lamar;
    using Microsoft.Extensions.Configuration;
    using TransactionProcessor.Bootstrapper;
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
        public void OperatorRegistry_PopulatesAutoApiLogonOperators()
        {
            OperatorRegistry registry = new(this.SetupMemoryConfiguration());
            Container container = new(registry);

            Assert.Equal(new[] { "Safaricom", "PataPawaPostPay" }, container.GetInstance<AutoLogonOperatorConfiguration>().OperatorIds);
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

        #endregion
    }
}
