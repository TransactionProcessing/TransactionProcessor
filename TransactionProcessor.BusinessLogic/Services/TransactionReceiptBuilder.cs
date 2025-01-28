namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.IO.Abstractions;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using EstateManagement.DataTransferObjects.Responses;
    using Models;

    public interface ITransactionReceiptBuilder
    {
        #region Methods

        /// <summary>
        /// Gets the email receipt message.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="merchant">The merchant.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<String> GetEmailReceiptMessage(Transaction transaction,
                                            EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant,
                                            String operatorName,
                                            CancellationToken cancellationToken);

        #endregion
    }

    public class TransactionReceiptBuilder : ITransactionReceiptBuilder
    {
        #region Fields

        /// <summary>
        /// The file system
        /// </summary>
        private readonly IFileSystem FileSystem;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionReceiptBuilder"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public TransactionReceiptBuilder(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the email receipt message.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="merchant">The merchant.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<String> GetEmailReceiptMessage(Transaction transaction,
                                                         EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant,
                                                         String operatorName,
                                                         CancellationToken cancellationToken)
        {
            IDirectoryInfo path = this.FileSystem.Directory.GetParent(Assembly.GetExecutingAssembly().Location);

            String fileData =
                await this.FileSystem.File.ReadAllTextAsync($"{path}/Receipts/Email/{operatorName}/TransactionAuthorised.html", cancellationToken);

            PropertyInfo[] transactonProperties = transaction.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Do the replaces for the transaction
            foreach (PropertyInfo propertyInfo in transactonProperties)
            {
                fileData = fileData.Replace($"[{propertyInfo.Name}]", propertyInfo.GetValue(transaction)?.ToString());
            }

            PropertyInfo[] merchantProperties = merchant.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            // Do the replaces for the merchant
            foreach (PropertyInfo propertyInfo in merchantProperties)
            {
                fileData = fileData.Replace($"[{propertyInfo.Name}]", propertyInfo.GetValue(merchant)?.ToString());
            }

            return fileData;
        }

        #endregion
    }
}