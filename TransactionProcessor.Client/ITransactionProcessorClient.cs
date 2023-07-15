namespace TransactionProcessor.Client
{
    using System;
    using System.Collections.Generic;
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
                                                                   Guid merchantId,
                                                                   CancellationToken cancellationToken);

        Task ProcessSettlement(String accessToken,
                               DateTime settlementDate,
                               Guid estateId,
                               Guid merchantId,
                               CancellationToken cancellationToken);

        Task ResendEmailReceipt(String accessToken,
                               Guid estateId,
                               Guid transactionId,
                               CancellationToken cancellationToken);

        Task<MerchantBalanceResponse> GetMerchantBalance(String accessToken,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         CancellationToken cancellationToken);

        Task<List<MerchantBalanceChangedEntryResponse>> GetMerchantBalanceHistory(String accessToken,
                                                                                  Guid estateId,
                                                                                  Guid merchantId,
                                                                                  DateTime startDate,
                                                                                  DateTime endDate,
                                                                                  CancellationToken cancellationToken);

        Task<GetVoucherResponse> GetVoucherByCode(String accessToken,
                                            Guid estateId,
                                            String voucherCode,
                                            CancellationToken cancellationToken);

        Task<GetVoucherResponse> GetVoucherByTransactionId(String accessToken,
                                                  Guid estateId,
                                                  Guid transactionId,
                                                  CancellationToken cancellationToken);

        Task<RedeemVoucherResponse> RedeemVoucher(String accessToken,
                                                  RedeemVoucherRequest redeemVoucherRequest,
                                                  CancellationToken cancellationToken);

        #endregion
    }
}