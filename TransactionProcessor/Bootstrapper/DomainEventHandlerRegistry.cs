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
        private const String EventHandlerConfiguration = "EventHandlerConfiguration";
        private const String EventHandlerConfigurationDomain = "EventHandlerConfigurationDomain";
        private const String EventHandlerConfigurationOrdered = "EventHandlerConfigurationOrdered";

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventHandlerRegistry"/> class.
        /// </summary>
        public DomainEventHandlerRegistry()
        {
            Dictionary<String, String[]> eventHandlersConfiguration =
                this.GetEventHandlerConfiguration($"AppSettings:{EventHandlerConfiguration}", EventHandlerConfiguration);
            Dictionary<String, String[]> eventHandlersConfigurationDomain =
                this.GetEventHandlerConfiguration($"AppSettings:{EventHandlerConfigurationDomain}", EventHandlerConfigurationDomain);
            Dictionary<String, String[]> eventHandlersConfigurationOrdered =
                this.GetEventHandlerConfiguration($"AppSettings:{EventHandlerConfigurationOrdered}", EventHandlerConfigurationOrdered);

            this.RegisterEventHandlers();
            this.RegisterProjections();
            this.RegisterResolvers(eventHandlersConfiguration, eventHandlersConfigurationDomain, eventHandlersConfigurationOrdered);
            this.AddSingleton<IDomainEventFactory<IDomainEvent>, DomainEventFactory>();
        }

        #endregion

        #region Methods

        private Dictionary<String, String[]> GetEventHandlerConfiguration(String sectionName,
                                                                          String registrationName)
        {
            Dictionary<String, String[]> eventHandlersConfiguration = new Dictionary<String, String[]>();

            if (Startup.Configuration != null)
            {
                IConfigurationSection section = Startup.Configuration.GetSection(sectionName);

                if (section != null)
                {
                    section.Bind(eventHandlersConfiguration);
                }

                this.Use(eventHandlersConfiguration).Named(registrationName);
            }

            return eventHandlersConfiguration;
        }

        private void RegisterEventHandlers()
        {
            this.AddSingleton<Func<Type, IDomainEventHandler>>(container => type =>
                                                                            {
                                                                                IDomainEventHandler handler = container.GetService(type) as IDomainEventHandler;
                                                                                return handler;
                                                                            });

            this.AddSingleton<Func<String, IDomainEventHandler>>(container => type =>
                                                                              {
                                                                                  return type switch
                                                                                  {
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
        }

        private void RegisterProjections()
        {
            this.AddSingleton<ProjectionHandler<MerchantBalanceState>>();
            this.AddSingleton<ProjectionHandler<VoucherState>>();
            this.AddSingleton<IProjection<MerchantBalanceState>, MerchantBalanceProjection>();
            this.AddSingleton<IProjection<VoucherState>, VoucherProjection>();
            this.AddSingleton<IStateDispatcher<MerchantBalanceState>, MerchantBalanceStateDispatcher>();
            this.AddSingleton<IStateDispatcher<VoucherState>, VoucherStateDispatcher>();
        }

        private void RegisterResolvers(Dictionary<String, String[]> eventHandlersConfiguration,
                                       Dictionary<String, String[]> eventHandlersConfigurationDomain,
                                       Dictionary<String, String[]> eventHandlersConfigurationOrdered)
        {
            this.For<IDomainEventHandlerResolver>().Use<DomainEventHandlerResolver>().Named("Main")
                .Ctor<Dictionary<String, String[]>>().Is(eventHandlersConfiguration).Singleton();
            this.For<IDomainEventHandlerResolver>().Use<DomainEventHandlerResolver>().Named("Domain")
                .Ctor<Dictionary<String, String[]>>().Is(eventHandlersConfigurationDomain).Singleton();
            this.For<IDomainEventHandlerResolver>().Use<DomainEventHandlerResolver>().Named("Ordered")
                .Ctor<Dictionary<String, String[]>>().Is(eventHandlersConfigurationOrdered).Singleton();
        }

        #endregion
    }
}
