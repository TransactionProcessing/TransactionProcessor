namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EstateManagement.DataTransferObjects.Responses;
    using Models;

    /// <summary>
    /// 
    /// </summary>
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
                                            MerchantResponse merchant,
                                            String operatorName, 
                                            CancellationToken cancellationToken);

        #endregion
    }
}