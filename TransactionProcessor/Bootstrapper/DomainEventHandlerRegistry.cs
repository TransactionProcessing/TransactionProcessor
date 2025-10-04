using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;

namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic.EventHandling;
    using Lamar;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ProjectionEngine.Dispatchers;
    using ProjectionEngine.EventHandling;
    using ProjectionEngine.ProjectionHandler;
    using ProjectionEngine.Projections;
    using ProjectionEngine.State;
    using Shared.EventStore.EventHandling;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class DomainEventHandlerRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventHandlerRegistry"/> class.
        /// </summary>
        public DomainEventHandlerRegistry()
        {
            Dictionary<String, String[]> eventHandlersConfiguration = new Dictionary<String, String[]>();
            Dictionary<String, String[]> eventHandlersConfigurationDomain= new Dictionary<String, String[]>();
            Dictionary<String, String[]> eventHandlersConfigurationOrdered = new Dictionary<String, String[]>();

            if (Startup.Configuration != null)
            {
                IConfigurationSection section = Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration");

                if (section != null)
                {
                    Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration").Bind(eventHandlersConfiguration);
                }

                this.Use(eventHandlersConfiguration).Named("EventHandlerConfiguration");

                section = Startup.Configuration.GetSection("AppSettings:EventHandlerConfigurationDomain");

                if (section != null)
                {
                    Startup.Configuration.GetSection("AppSettings:EventHandlerConfigurationDomain").Bind(eventHandlersConfigurationDomain);
                }

                this.Use(eventHandlersConfigurationDomain).Named("EventHandlerConfigurationDomain");

                section = Startup.Configuration.GetSection("AppSettings:EventHandlerConfigurationOrdered");

                if (section != null)
                {
                    Startup.Configuration.GetSection("AppSettings:EventHandlerConfigurationOrdered").Bind(eventHandlersConfigurationOrdered);
                }

                this.Use(eventHandlersConfigurationOrdered).Named("EventHandlerConfigurationOrdered");
            }

            this.AddSingleton<Func<Type, IDomainEventHandler>>(container => type =>
                                                                            {
                                                                                IDomainEventHandler handler = container.GetService(type) as IDomainEventHandler;
                                                                                return handler;
                                                                            });

            this.AddSingleton<Func<String, IDomainEventHandler>>(container => type => {
                                                                                  return type switch{
                                                                                      "VoucherState" => container.GetService<StateProjectionEventHandler<VoucherState>>(),
                                                                                      _ => container.GetService<StateProjectionEventHandler<MerchantBalanceState>>()
                                                                                  };
                                                                              });

            this.AddSingleton<TransactionProcessor.ProjectionEngine.EventHandling.EventHandler>();
            this.AddSingleton<ReadModelDomainEventHandler>();
            this.AddSingleton<TransactionDomainEventHandler>();
            this.AddSingleton<VoucherDomainEventHandler>();
            this.AddSingleton<MerchantDomainEventHandler>();
            this.AddSingleton<StateProjectionEventHandler<MerchantBalanceState>>();
            this.AddSingleton<StateProjectionEventHandler<VoucherState>>();

            this.AddSingleton<ProjectionHandler<MerchantBalanceState>>();
            this.AddSingleton<ProjectionHandler<VoucherState>>();
            this.AddSingleton<IProjection<MerchantBalanceState>, MerchantBalanceProjection>();
            this.AddSingleton<IProjection<VoucherState>, VoucherProjection>();
            this.AddSingleton<IStateDispatcher<MerchantBalanceState>, MerchantBalanceStateDispatcher>();
            this.AddSingleton<IStateDispatcher<VoucherState>, VoucherStateDispatcher>();

            this.For<IDomainEventHandlerResolver>().Use<DomainEventHandlerResolver>().Named("Main")
                .Ctor<Dictionary<String, String[]>>().Is(eventHandlersConfiguration).Singleton();
            this.For<IDomainEventHandlerResolver>().Use<DomainEventHandlerResolver>().Named("Domain")
                .Ctor<Dictionary<String, String[]>>().Is(eventHandlersConfigurationDomain).Singleton();
            this.For<IDomainEventHandlerResolver>().Use<DomainEventHandlerResolver>().Named("Ordered")
                .Ctor<Dictionary<String, String[]>>().Is(eventHandlersConfigurationOrdered).Singleton();

            this.AddSingleton<IDomainEventFactory<IDomainEvent>, DomainEventFactory>();
        }

        #endregion
    }
}