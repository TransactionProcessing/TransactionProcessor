﻿namespace TransactionProcessor.Bootstrapper
{
    using System.Diagnostics.CodeAnalysis;
    using BusinessLogic.Services;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class DomainServiceRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainServiceRegistry"/> class.
        /// </summary>
        public DomainServiceRegistry()
        {
            this.AddSingleton<ITransactionDomainService, TransactionDomainService>();
            this.AddSingleton<ISettlementDomainService, SettlementDomainService>();
            this.AddSingleton<IVoucherDomainService, VoucherDomainService>();
            this.AddSingleton<ITransactionValidationService, TransactionValidationService>();
            this.AddSingleton<IFloatDomainService, FloatDomainService>();
            this.AddSingleton<IEstateDomainService, EstateDomainService>();
            this.AddSingleton<IOperatorDomainService, OperatorDomainService>();
            this.AddSingleton<IContractDomainService, ContractDomainService>();
            this.AddSingleton<IMerchantDomainService, MerchantDomainService>();
            this.AddSingleton<IMerchantStatementDomainService, MerchantStatementDomainService>();
        }

        #endregion
    }
}