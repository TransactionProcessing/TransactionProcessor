using SimpleResults;

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
        
        Task<Result<SerialisedMessage>> PerformTransaction(String accessToken, 
                                                           SerialisedMessage transactionRequest,
                                                           CancellationToken cancellationToken);

        Task<Result<SettlementResponse>> GetSettlementByDate(String accessToken, 
                                                                   DateTime settlementDate,
                                                                   Guid estateId,
                                                                   Guid merchantId,
                                                                   CancellationToken cancellationToken);

        Task<Result> ProcessSettlement(String accessToken,
                               DateTime settlementDate,
                               Guid estateId,
                               Guid merchantId,
                               CancellationToken cancellationToken);

        Task<Result> ResendEmailReceipt(String accessToken,
                                        Guid estateId,
                                        Guid transactionId,
                                        CancellationToken cancellationToken);

        Task<Result<MerchantBalanceResponse>> GetMerchantBalance(String accessToken,
                                                         Guid estateId,
                                                         Guid merchantId,
                                                         CancellationToken cancellationToken,
                                                         Boolean liveBalance = true);

        Task<Result<List<MerchantBalanceChangedEntryResponse>>> GetMerchantBalanceHistory(String accessToken,
                                                                                  Guid estateId,
                                                                                  Guid merchantId,
                                                                                  DateTime startDate,
                                                                                  DateTime endDate,
                                                                                  CancellationToken cancellationToken);

        Task<Result<GetVoucherResponse>> GetVoucherByCode(String accessToken,
                                            Guid estateId,
                                            String voucherCode,
                                            CancellationToken cancellationToken);

        Task<Result<GetVoucherResponse>> GetVoucherByTransactionId(String accessToken,
                                                  Guid estateId,
                                                  Guid transactionId,
                                                  CancellationToken cancellationToken);

        Task<Result<RedeemVoucherResponse>> RedeemVoucher(String accessToken,
                                                  RedeemVoucherRequest redeemVoucherRequest,
                                                  CancellationToken cancellationToken);

        Task<Result> CreateFloatForContractProduct(String accessToken,
                                                                                  Guid estateId,
                         CreateFloatForContractProductRequest createFloatForContractProductRequest,
                         CancellationToken cancellationToken);

        Task<Result> RecordFloatCreditPurchase(String accessToken,
                                       Guid estateId,
                                       RecordFloatCreditPurchaseRequest recordFloatCreditPurchaseRequest,
                                       CancellationToken cancellationToken);

        #endregion
    }
}