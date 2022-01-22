namespace TransactionProcessor.Bootstrapper
{
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
        }

        #endregion
    }
}