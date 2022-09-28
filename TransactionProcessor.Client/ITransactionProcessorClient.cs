namespace TransactionProcessor.Client
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;

    public interface ITransactionProcessorClient
    {
        #region Methods
        
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

        Task ResendEmailReceipt(String accessToken,
                               Guid estateId,
                               Guid transactionId,
                               CancellationToken cancellationToken);

        #endregion
    }
}