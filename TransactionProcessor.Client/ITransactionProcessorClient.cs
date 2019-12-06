namespace TransactionProcessor.Client
{
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;

    public interface ITransactionProcessorClient
    {
        #region Methods

        /// <summary>
        /// Performs the transaction.
        /// </summary>
        /// <param name="transactionRequest">The transaction request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<SerialisedMessage> PerformTransaction(SerialisedMessage transactionRequest,
                                                   CancellationToken cancellationToken);

        #endregion
    }
}