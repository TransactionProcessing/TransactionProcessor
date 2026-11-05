using System;
using Shared.General;
using SimpleResults;
using FluentValidation;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Settlement;
using TransactionProcessor.ProjectionEngine.Models;

namespace TransactionProcessor.Bootstrapper
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic.RequestHandlers;
    using BusinessLogic.Validation;
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
            this.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(EstateRequestHandler).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            this.Scan(scanner =>
            {
                scanner.AssemblyContainingType<CreateEstateCommandValidator>();
                scanner.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
            });
            this.AddSingleton<Func<String, String>>(container => (serviceName) => ConfigurationReader.GetBaseServerUri(serviceName).OriginalString);
        }
    }
}
