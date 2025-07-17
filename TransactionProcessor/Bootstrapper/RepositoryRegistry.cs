using TransactionProcessor.Aggregates;
using TransactionProcessor.BusinessLogic.Services;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Repository;

namespace TransactionProcessor.Bootstrapper
{
    using Lamar;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using ProjectionEngine.Database.Database;
    using ProjectionEngine.Projections;
    using ProjectionEngine.Repository;
    using ProjectionEngine.State;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework;
    using Shared.EntityFramework.ConnectionStringConfiguration;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.EventStore.SubscriptionWorker;
    using Shared.General;
    using Shared.Repositories;
    using System;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

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
            String eventStoreConnectionString = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString");

            this.AddEventStoreProjectionManagementClient(eventStoreConnectionString);
            this.AddEventStorePersistentSubscriptionsClient(eventStoreConnectionString);

            this.AddEventStoreClient(eventStoreConnectionString);

            this.AddSingleton<Func<String, Int32, ISubscriptionRepository>>(cont => (esConnString, cacheDuration) => {
                return SubscriptionRepository.Create(esConnString, cacheDuration);
            });

            this.AddSingleton(typeof(IDbContextResolver<>), typeof(DbContextResolver<>));
            if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") || Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase") == true)
            {
                this.AddDbContext<EstateManagementContext>(builder => builder.UseInMemoryDatabase("TransactionProcessorReadModel"));
            }
            else
            {
                this.AddDbContext<EstateManagementContext>(options =>
                    options.UseSqlServer(ConfigurationReader.GetConnectionString("TransactionProcessorReadModel")));
            }

            this.AddTransient<IEventStoreContext, EventStoreContext>();

            this.AddSingleton<IAggregateService, AggregateService>();
            this.AddSingleton<IAggregateRepositoryResolver, AggregateRepositoryResolver>();
            this.AddSingleton<IAggregateRepository<TransactionAggregate, DomainEvent>,
                AggregateRepository<TransactionAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<ReconciliationAggregate, DomainEvent>,
                AggregateRepository<ReconciliationAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<SettlementAggregate, DomainEvent>,
                AggregateRepository<SettlementAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<VoucherAggregate, DomainEvent>, AggregateRepository<VoucherAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<FloatAggregate, DomainEvent>, AggregateRepository<FloatAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<FloatActivityAggregate, DomainEvent>, AggregateRepository<FloatActivityAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<EstateAggregate, DomainEvent>, AggregateRepository<EstateAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<OperatorAggregate, DomainEvent>, AggregateRepository<OperatorAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<ContractAggregate, DomainEvent>, AggregateRepository<ContractAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<MerchantAggregate, DomainEvent>, AggregateRepository<MerchantAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<MerchantDepositListAggregate, DomainEvent>, AggregateRepository<MerchantDepositListAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<MerchantStatementAggregate, DomainEvent>, AggregateRepository<MerchantStatementAggregate, DomainEvent>>();
            this.AddSingleton<IAggregateRepository<MerchantStatementForDateAggregate, DomainEvent>, AggregateRepository<MerchantStatementForDateAggregate, DomainEvent>>();

            this.AddSingleton<IProjectionStateRepository<MerchantBalanceState>, MerchantBalanceStateRepository>();
            this.AddSingleton<IProjectionStateRepository<VoucherState>, VoucherStateRepository>();
            this.AddSingleton<ITransactionProcessorReadRepository, TransactionProcessorReadRepository>();
            this.AddSingleton<ITransactionProcessorReadModelRepository, TransactionProcessorReadModelRepository>();
            this.AddSingleton<IProjection<MerchantBalanceState>, MerchantBalanceProjection>();
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
            
            // Default to SQL Server
            builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = databaseName
            };

            return builder.ToString();
        }

        #endregion
    }
}