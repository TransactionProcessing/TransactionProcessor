namespace TransactionProcessor.Bootstrapper
{
    using BusinessLogic.Services;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;
    using System.Diagnostics.CodeAnalysis;

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
        }

        #endregion
    }
}