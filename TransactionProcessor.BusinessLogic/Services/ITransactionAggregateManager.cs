namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;
    using OperatorInterfaces;
    using TransactionAggregate;

    /// <summary>
    /// 
    /// </summary>
    public interface ITransactionAggregateManager
    {
        #region Methods

        Task AddFee(Guid estateId,
                    Guid transactionId,
                    CalculatedFee calculatedFee,
                    CancellationToken cancellationToken);

        Task AddProductDetails(Guid estateId,
                               Guid transactionId,
                               Guid contractId,
                               Guid productId,
                               CancellationToken cancellationToken);

        Task AddSettledFee(Guid estateId,
                           Guid transactionId,
                           CalculatedFee calculatedFee,
                           DateTime settlementDueDate,
                           DateTime settledDateTime,
                           CancellationToken cancellationToken);

        Task AddTransactionSource(Guid estateId,
                                  Guid transactionId,
                                  TransactionSource transactionSource,
                                  CancellationToken cancellationToken);

        Task AuthoriseTransaction(Guid estateId,
                                  Guid transactionId,
                                  String operatorIdentifier,
                                  OperatorResponse operatorResponse,
                                  TransactionResponseCode transactionResponseCode,
                                  String responseMessage,
                                  CancellationToken cancellationToken);

        Task AuthoriseTransactionLocally(Guid estateId,
                                         Guid transactionId,
                                         String authorisationCode,
                                         (String responseMessage, TransactionResponseCode responseCode) validationResult,
                                         CancellationToken cancellationToken);

        Task CompleteTransaction(Guid estateId,
                                 Guid transactionId,
                                 CancellationToken cancellationToken);

        Task DeclineTransaction(Guid estateId,
                                Guid transactionId,
                                String operatorIdentifier,
                                OperatorResponse operatorResponse,
                                TransactionResponseCode transactionResponseCode,
                                String responseMessage,
                                CancellationToken cancellationToken);

        Task DeclineTransactionLocally(Guid estateId,
                                       Guid transactionId,
                                       (String responseMessage, TransactionResponseCode responseCode) validationResult,
                                       CancellationToken cancellationToken);

        Task<TransactionAggregate> GetAggregate(Guid estateId,
                                                Guid transactionId,
                                                CancellationToken cancellationToken);

        Task RecordAdditionalRequestData(Guid estateId,
                                         Guid transactionId,
                                         String operatorIdentifier,
                                         Dictionary<String, String> additionalTransactionRequestMetadata,
                                         CancellationToken cancellationToken);

        Task RecordAdditionalResponseData(Guid estateId,
                                          Guid transactionId,
                                          String operatorIdentifier,
                                          Dictionary<String, String> additionalTransactionResponseMetadata,
                                          CancellationToken cancellationToken);

        Task RequestEmailReceipt(Guid estateId,
                                 Guid transactionId,
                                 String customerEmailAddress,
                                 CancellationToken cancellationToken);

        Task ResendReceipt(Guid estateId,
                           Guid transactionId,
                           CancellationToken cancellationToken);

        Task StartTransaction(Guid transactionId,
                              DateTime transactionDateTime,
                              String transactionNumber,
                              TransactionType transactionType,
                              String transactionReference,
                              Guid estateId,
                              Guid merchantId,
                              String deviceIdentifier,
                              Decimal? transactionAmount,
                              CancellationToken cancellationToken);

        #endregion
    }
}