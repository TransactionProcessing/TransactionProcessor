namespace TransactionProcessor.Tests.General
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using Autofac.Core;

    /// <summary>
    /// 
    /// </summary>
    public static class ScopeExtensions
    {
        #region Methods

        /// <summary>
        /// Filters the specified ignored assemblies.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="ignoredAssemblies">The ignored assemblies.</param>
        /// <returns></returns>
        public static IList<IServiceWithType> Filter(this IEnumerable<IServiceWithType> services,
                                                     IEnumerable<String> ignoredAssemblies)
        {
            return services.Where(serviceWithType => ignoredAssemblies.All(ignored => ignored != serviceWithType.ServiceType.FullName)).ToList();
        }

        /// <summary>
        /// Resolves all.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="ignoredAssemblies">The ignored assemblies.</param>
        /// <returns></returns>
        public static IList<Object> ResolveAll(this ILifetimeScope scope,
                                               IEnumerable<String> ignoredAssemblies)
        {
            var services = scope.ComponentRegistry.Registrations.SelectMany(x => x.Services).OfType<IServiceWithType>().Filter(ignoredAssemblies).ToList();

            foreach (var serviceWithType in services)
            {
                try
                {
                    scope.Resolve(serviceWithType.ServiceType);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return services.Select(x => x.ServiceType).Select(scope.Resolve).ToList();
        }

        #endregion
    }
}