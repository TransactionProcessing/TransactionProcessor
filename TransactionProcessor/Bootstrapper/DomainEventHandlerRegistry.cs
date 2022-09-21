namespace TransactionProcessor.Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic.EventHandling;
    using Lamar;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
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

            if (Startup.Configuration != null)
            {
                IConfigurationSection section = Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration");

                if (section != null)
                {
                    Startup.Configuration.GetSection("AppSettings:EventHandlerConfiguration").Bind(eventHandlersConfiguration);
                }
            }

            this.AddSingleton(eventHandlersConfiguration);

            this.AddSingleton<Func<Type, IDomainEventHandler>>(container => type =>
                                                                            {
                                                                                IDomainEventHandler handler = container.GetService(type) as IDomainEventHandler;
                                                                                return handler;
                                                                            });

            this.AddSingleton<TransactionDomainEventHandler>();
            this.AddSingleton<IDomainEventHandlerResolver, DomainEventHandlerResolver>();
        }

        #endregion
    }
}