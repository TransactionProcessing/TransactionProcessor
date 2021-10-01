namespace TransactionProcessor.Client
{
    using System;
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
        Task<SerialisedMessage> PerformTransaction(String accessToken, 
                                                   SerialisedMessage transactionRequest,
                                                   CancellationToken cancellationToken);

        Task<SettlementResponse> GetSettlementByDate(String accessToken, 
                                                                   DateTime settlementDate,
                                                                   Guid estateId,
                                                                   CancellationToken cancellationToken);

        Task ProcessSettlement(String accessToken,
                               DateTime settlementDate,
                               Guid estateId,
                               CancellationToken cancellationToken);

        #endregion
    }
}