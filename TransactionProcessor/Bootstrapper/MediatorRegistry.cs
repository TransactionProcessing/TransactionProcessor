﻿namespace TransactionProcessor.Bootstrapper
{
    using BusinessLogic.RequestHandlers;
    using BusinessLogic.Requests;
    using Lamar;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Models;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class MediatorRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediatorRegistry"/> class.
        /// </summary>
        public MediatorRegistry()
        {
            this.AddTransient<IMediator, Mediator>();

            // request & notification handlers
            this.AddTransient<ServiceFactory>(context => { return t => context.GetService(t); });

            this.AddSingleton<IRequestHandler<ProcessLogonTransactionRequest, ProcessLogonTransactionResponse>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<ProcessSaleTransactionRequest, ProcessSaleTransactionResponse>, TransactionRequestHandler>();
            this.AddSingleton<IRequestHandler<ProcessReconciliationRequest, ProcessReconciliationTransactionResponse>, TransactionRequestHandler>();

            this.AddSingleton<IRequestHandler<ProcessSettlementRequest, ProcessSettlementResponse>, SettlementRequestHandler>();
        }

        #endregion
    }
}