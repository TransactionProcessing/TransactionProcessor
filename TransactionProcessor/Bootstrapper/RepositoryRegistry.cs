namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Net.Http;
    using System.Net.Security;
    using BusinessLogic.Services;
    using Lamar;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ReconciliationAggregate;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework.ConnectionStringConfiguration;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.EventStore.Extensions;
    using Shared.General;
    using Shared.Repositories;
    using TransactionAggregate;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    public class RepositoryRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryRegistry"/> class.
        /// </summary>
        public RepositoryRegistry()
        {
            Boolean useConnectionStringConfig = bool.Parse(ConfigurationReader.GetValue("AppSettings", "UseConnectionStringConfig"));

            if (useConnectionStringConfig)
            {
                String connectionStringConfigurationConnString = ConfigurationReader.GetConnectionString("ConnectionStringConfiguration");
                this.AddSingleton<IConnectionStringConfigurationRepository, ConnectionStringConfigurationRepository>();
                this.AddTransient(c => { return new ConnectionStringConfigurationContext(connectionStringConfigurationConnString); });

                // TODO: Read this from a the database and set
            }
            else
            {
                Boolean insecureES = Startup.Configuration.GetValue<Boolean>("EventStoreSettings:Insecure");

                Func<SocketsHttpHandler> CreateHttpMessageHandler = () => new SocketsHttpHandler
                                                                          {
                                                                              SslOptions = new SslClientAuthenticationOptions
                                                                                           {
                                                                                               RemoteCertificateValidationCallback = (sender,
                                                                                                   certificate,
                                                                                                   chain,
                                                                                                   errors) => {
                                                                                                   return true;
                                                                                               }
                                                                                           }
                                                                          };

                this.AddEventStoreProjectionManagerClient(Startup.ConfigureEventStoreSettings);
                this.AddEventStorePersistentSubscriptionsClient(Startup.ConfigureEventStoreSettings);

                if (insecureES)
                {
                    this.AddInSecureEventStoreClient(Startup.EventStoreClientSettings.ConnectivitySettings.Address, CreateHttpMessageHandler);
                }
                else
                {
                    this.AddEventStoreClient(Startup.EventStoreClientSettings.ConnectivitySettings.Address, CreateHttpMessageHandler);
                }
            }

            this.AddTransient<IEventStoreContext, EventStoreContext>();

            this.AddSingleton<ITransactionAggregateManager, TransactionAggregateManager>();
            this.AddSingleton<IAggregateRepository<TransactionAggregate, DomainEvent>,
                AggregateRepository<TransactionAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<ReconciliationAggregate, DomainEvent>,
                AggregateRepository<ReconciliationAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<SettlementAggregate, DomainEvent>,
                AggregateRepository<SettlementAggregate, DomainEvent>>();
        }

        #endregion
    }
}