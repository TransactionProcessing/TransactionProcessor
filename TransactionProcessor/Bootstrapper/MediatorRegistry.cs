using System;
using Shared.General;
using SimpleResults;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Settlement;
using TransactionProcessor.ProjectionEngine.Models;

namespace TransactionProcessor.Bootstrapper
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic.RequestHandlers;
    using BusinessLogic.Requests;
    using Lamar;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using TransactionProcessor.ProjectionEngine.State;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class MediatorRegistry : ServiceRegistry
    {
        public MediatorRegistry()
        {
            this.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EstateRequestHandler).Assembly));
            this.AddSingleton<Func<String, String>>(container => (serviceName) => ConfigurationReader.GetBaseServerUri(serviceName).OriginalString);
        }
    }
}