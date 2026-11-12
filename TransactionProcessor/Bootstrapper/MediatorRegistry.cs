using System;
using TransactionProcessing.Shared.General;
using TransactionProcessing.UnionResults;
using FluentValidation;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models.Settlement;
using TransactionProcessor.ProjectionEngine.Models;

namespace TransactionProcessor.Bootstrapper
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic.RequestHandlers;
    using BusinessLogic.Requests;
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
                cfg.AddBehavior<IPipelineBehavior<EstateCommands.CreateEstateCommand, Result<Guid>>,
                    ValidationBehavior<EstateCommands.CreateEstateCommand, Guid>>();
                cfg.AddBehavior<IPipelineBehavior<MerchantCommands.CreateMerchantCommand, Result<Guid>>,
                    ValidationBehavior<MerchantCommands.CreateMerchantCommand, Guid>>();
                cfg.AddBehavior<IPipelineBehavior<OperatorCommands.CreateOperatorCommand, Result<Guid>>,
                    ValidationBehavior<OperatorCommands.CreateOperatorCommand, Guid>>();
                cfg.AddBehavior<IPipelineBehavior<FloatCommands.CreateFloatCommand, Result<Guid>>,
                    ValidationBehavior<FloatCommands.CreateFloatCommand, Guid>>();
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
