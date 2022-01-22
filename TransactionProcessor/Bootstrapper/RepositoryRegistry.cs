namespace TransactionProcessor.Bootstrapper
{
    using System;
    using BusinessLogic.Services;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;
    using ReconciliationAggregate;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework.ConnectionStringConfiguration;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
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
                this.AddEventStoreClient(Startup.ConfigureEventStoreSettings);
                this.AddEventStoreProjectionManagementClient(Startup.ConfigureEventStoreSettings);
                this.AddEventStorePersistentSubscriptionsClient(Startup.ConfigureEventStoreSettings);
            }

            this.AddTransient<IEventStoreContext, EventStoreContext>();

            this.AddSingleton<ITransactionAggregateManager, TransactionAggregateManager>();
            this.AddSingleton<IAggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>,
                AggregateRepository<TransactionAggregate, DomainEventRecord.DomainEvent>>();
            this.AddSingleton<IAggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>,
                AggregateRepository<ReconciliationAggregate, DomainEventRecord.DomainEvent>>();
            this.AddSingleton<IAggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>,
                AggregateRepository<SettlementAggregate, DomainEventRecord.DomainEvent>>();
        }

        #endregion
    }
}