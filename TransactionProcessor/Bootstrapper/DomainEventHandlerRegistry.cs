namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic.EventHandling;
    using Lamar;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ProjectionEngine;
    using ProjectionEngine.Database;
    using ProjectionEngine.Dispatchers;
    using ProjectionEngine.EventHandling;
    using ProjectionEngine.ProjectionHandler;
    using ProjectionEngine.Projections;
    using ProjectionEngine.State;
    using Shared.EntityFramework;
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
            Dictionary<String, String[]> eventHandlersConfigurationOrdered = new Dictionary<String, String[]>();

            if (Startup.Configuration != null)
            {
                IConfigurationSection section = Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration");

                if (section != null)
                {
                    Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration").Bind(eventHandlersConfiguration);
                }

                //this.AddSingleton(eventHandlersConfiguration);
                this.Use(eventHandlersConfiguration).Named("Concurrent");

                section = Startup.Configuration.GetSection("AppSettings:EventHandlerConfigurationOrdered");

                if (section != null)
                {
                    Startup.Configuration.GetSection("AppSettings:EventHandlerConfigurationOrdered").Bind(eventHandlersConfigurationOrdered);
                }
                
                this.Use(eventHandlersConfigurationOrdered).Named("Ordered");
            }

            this.AddSingleton<Func<Type, IDomainEventHandler>>(container => type =>
                                                                            {
                                                                                IDomainEventHandler handler = container.GetService(type) as IDomainEventHandler;
                                                                                return handler;
                                                                            });

            this.AddSingleton<Func<String, IDomainEventHandler>>(container => type => {
                                                                                  return container.GetService<StateProjectionEventHandler<MerchantBalanceState>>();
                                                                              });

            this.AddSingleton<TransactionProcessor.ProjectionEngine.EventHandling.EventHandler>();
            this.AddSingleton<TransactionDomainEventHandler>();
            this.AddSingleton<StateProjectionEventHandler<MerchantBalanceState>>();

            this.AddSingleton<ProjectionHandler<MerchantBalanceState>>();
            this.AddSingleton<IProjection<MerchantBalanceState>, MerchantBalanceProjection>();
            this.AddSingleton<IStateDispatcher<MerchantBalanceState>, MerchantBalanceStateDispatcher>();

            this.For<IDomainEventHandlerResolver>().Use<DomainEventHandlerResolver>().Named("Concurrent")
                .Ctor<Dictionary<String, String[]>>().Is(eventHandlersConfiguration).Singleton();
            this.For<IDomainEventHandlerResolver>().Use<DomainEventHandlerResolver>().Named("Ordered")
                .Ctor<Dictionary<String, String[]>>().Is(eventHandlersConfigurationOrdered).Singleton();
        }

        #endregion
    }
}