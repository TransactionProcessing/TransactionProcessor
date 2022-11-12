namespace TransactionProcessor.Bootstrapper
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO.Abstractions;
    using BusinessLogic.Manager;
    using BusinessLogic.Services;
    using Factories;
    using Lamar;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Lamar.ServiceRegistry" />
    [ExcludeFromCodeCoverage]
    public class MiscRegistry : ServiceRegistry
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MiscRegistry"/> class.
        /// </summary>
        public MiscRegistry()
        {
            this.AddSingleton<IModelFactory, ModelFactory>();
            this.AddSingleton<ITransactionReceiptBuilder, TransactionReceiptBuilder>();
            this.AddSingleton<IFileSystem, FileSystem>();
            this.AddSingleton<IFeeCalculationManager, FeeCalculationManager>();
            this.AddSingleton<IVoucherManagementManager, VoucherManagementManager>();
        }

        #endregion
    }
}