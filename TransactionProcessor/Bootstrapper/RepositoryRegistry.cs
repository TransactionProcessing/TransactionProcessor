using TransactionProcessor.FloatAggregate;

namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Net.Security;
    using System.Threading.Tasks;
    using System.Threading;
    using BusinessLogic.Services;
    using Lamar;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using MySqlConnector;
    using ProjectionEngine;
    using ProjectionEngine.Database;
    using ProjectionEngine.Dispatchers;
    using ProjectionEngine.Projections;
    using ReconciliationAggregate;
    using SettlementAggregates;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework;
    using Shared.EntityFramework.ConnectionStringConfiguration;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.EventStore.Extensions;
    using Shared.General;
    using Shared.Repositories;
    using TransactionAggregate;
    using ConnectionStringType = Shared.Repositories.ConnectionStringType;
    using EstateManagement.Database.Contexts;
    using ProjectionEngine.Database.Database;
    using ProjectionEngine.Repository;
    using ProjectionEngine.State;
    using Shared.EventStore.SubscriptionWorker;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
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
                String connectionString = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString");

                this.AddEventStoreProjectionManagementClient(connectionString);
                this.AddEventStorePersistentSubscriptionsClient(connectionString);

                this.AddEventStoreClient(connectionString);

                this.AddSingleton<IConnectionStringConfigurationRepository, ConfigurationReaderConnectionStringRepository>();
            }

            this.AddTransient<IEventStoreContext, EventStoreContext>();
            
            this.AddSingleton<IAggregateRepository<TransactionAggregate, DomainEvent>,
                AggregateRepository<TransactionAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<ReconciliationAggregate, DomainEvent>,
                AggregateRepository<ReconciliationAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<SettlementAggregate, DomainEvent>,
                AggregateRepository<SettlementAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<VoucherAggregate.VoucherAggregate, DomainEvent>, AggregateRepository<VoucherAggregate.VoucherAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<FloatAggregate.FloatAggregate, DomainEvent>, AggregateRepository<FloatAggregate.FloatAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<FloatActivityAggregate, DomainEvent>, AggregateRepository<FloatActivityAggregate, DomainEvent>>();


            this.AddSingleton<IProjectionStateRepository<MerchantBalanceState>, MerchantBalanceStateRepository>();
            this.AddSingleton<IProjectionStateRepository<VoucherState>, VoucherStateRepository>();
            this.AddSingleton<ITransactionProcessorReadRepository, TransactionProcessorReadRepository>();
            this.AddSingleton<IProjection<MerchantBalanceState>, MerchantBalanceProjection>();

            this.AddSingleton<IDbContextFactory<TransactionProcessorGenericContext>, DbContextFactory<TransactionProcessorGenericContext>>();
            this.AddSingleton<IDbContextFactory<EstateManagementGenericContext>, DbContextFactory<EstateManagementGenericContext>>();

            this.AddSingleton<Func<String, TransactionProcessorGenericContext>>(cont => connectionString =>
                                                                                   {
                                                                                       String databaseEngine =
                                                                                           ConfigurationReader.GetValue("AppSettings", "DatabaseEngine");

                                                                                       return databaseEngine switch
                                                                                       {
                                                                                           "MySql" => new TransactionProcessorMySqlContext(connectionString),
                                                                                           "SqlServer" => new TransactionProcessorSqlServerContext(connectionString),
                                                                                           _ => throw new
                                                                                               NotSupportedException($"Unsupported Database Engine {databaseEngine}")
                                                                                       };
                                                                                   });
            this.AddSingleton<Func<String, EstateManagementGenericContext>>(cont => connectionString =>
                                                                                    {
                                                                                        String databaseEngine =
                                                                                            ConfigurationReader.GetValue("AppSettings", "DatabaseEngine");

                                                                                        return databaseEngine switch
                                                                                        {
                                                                                            "MySql" => new EstateManagementMySqlContext(connectionString),
                                                                                            "SqlServer" => new EstateManagementSqlServerContext(connectionString),
                                                                                            _ => throw new
                                                                                                NotSupportedException($"Unsupported Database Engine {databaseEngine}")
                                                                                        };
                                                                                    });

            this.AddSingleton<Func<String, Int32, ISubscriptionRepository>>(cont => (esConnString, cacheDuration) => {
                                                                                       return SubscriptionRepository.Create(esConnString, cacheDuration);
                                                                                   });
        }

        #endregion
    }

    [ExcludeFromCodeCoverage]
    public class ConfigurationReaderConnectionStringRepository : IConnectionStringConfigurationRepository
    {
        #region Methods

        public async Task CreateConnectionString(String externalIdentifier,
                                                 String connectionStringIdentifier,
                                                 String connectionString,
                                                 CancellationToken cancellationToken)
        {
            throw new NotImplementedException("This is only required to complete the interface");
        }
        
        public async Task DeleteConnectionStringConfiguration(String externalIdentifier,
                                                              String connectionStringIdentifier,
                                                              CancellationToken cancellationToken)
        {
            throw new NotImplementedException("This is only required to complete the interface");
        }

        public async Task<String> GetConnectionString(String externalIdentifier,
                                                      String connectionStringIdentifier,
                                                      CancellationToken cancellationToken)
        {
            String connectionString = string.Empty;
            String databaseName = string.Empty;

            String databaseEngine = ConfigurationReader.GetValue("AppSettings", "DatabaseEngine");

            databaseName = $"{connectionStringIdentifier}{externalIdentifier}";
                    connectionString = ConfigurationReader.GetConnectionString(connectionStringIdentifier);

            DbConnectionStringBuilder builder = null;

            if (databaseEngine == "MySql")
            {
                builder = new MySqlConnectionStringBuilder(connectionString)
                {
                    Database = databaseName
                };
            }
            else
            {
                // Default to SQL Server
                builder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = databaseName
                };
            }

            return builder.ToString();
        }

        #endregion
    }
}