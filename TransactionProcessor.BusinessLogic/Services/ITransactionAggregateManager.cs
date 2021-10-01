namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EventHandling;
    using Models;
    using OperatorInterfaces;
    using TransactionAggregate;

    /// <summary>
    /// 
    /// </summary>
    public interface ITransactionAggregateManager
    {
        #region Methods

        /// <summary>
        /// Adds the product details.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task AddProductDetails(Guid estateId,
                               Guid transactionId,
                               Guid contractId,
                               Guid productId,
                               CancellationToken cancellationToken);

        /// <summary>
        /// Adds the fee.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="calculatedFee">The calculated fee.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task AddFee(Guid estateId,
                    Guid transactionId,
                    CalculatedFee calculatedFee,
                    CancellationToken cancellationToken);


        Task AddSettledFee(Guid estateId,
                    Guid transactionId,
                    CalculatedFee calculatedFee,
                    DateTime settlementDueDate,
                    DateTime settledDateTime,
                    CancellationToken cancellationToken);

        /// <summary>
        /// Authorises the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="operatorResponse">The operator response.</param>
        /// <param name="transactionResponseCode">The transaction response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task AuthoriseTransaction(Guid estateId,
                                  Guid transactionId,
                                  String operatorIdentifier,
                                  OperatorResponse operatorResponse,
                                  TransactionResponseCode transactionResponseCode,
                                  String responseMessage,
                                  CancellationToken cancellationToken);

        /// <summary>
        /// Authorises the transaction locally.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="authorisationCode">The authorisation code.</param>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task AuthoriseTransactionLocally(Guid estateId,
                                         Guid transactionId,
                                         String authorisationCode,
                                         (String responseMessage, TransactionResponseCode responseCode) validationResult,
                                         CancellationToken cancellationToken);

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task CompleteTransaction(Guid estateId,
                                 Guid transactionId,
                                 CancellationToken cancellationToken);

        /// <summary>
        /// Declines the transaction.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="operatorResponse">The operator response.</param>
        /// <param name="transactionResponseCode">The transaction response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task DeclineTransaction(Guid estateId,
                                Guid transactionId,
                                String operatorIdentifier,
                                OperatorResponse operatorResponse,
                                TransactionResponseCode transactionResponseCode,
                                String responseMessage,
                                CancellationToken cancellationToken);

        /// <summary>
        /// Declines the transaction locally.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task DeclineTransactionLocally(Guid estateId,
                                       Guid transactionId,
                                       (String responseMessage, TransactionResponseCode responseCode) validationResult,
                                       CancellationToken cancellationToken);

        /// <summary>
        /// Gets the aggregate.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<TransactionAggregate> GetAggregate(Guid estateId,
                                                Guid transactionId,
                                                CancellationToken cancellationToken);

        /// <summary>
        /// Records the additional request data.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="additionalTransactionRequestMetadata">The additional transaction request metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task RecordAdditionalRequestData(Guid estateId,
                                         Guid transactionId,
                                         String operatorIdentifier,
                                         Dictionary<String, String> additionalTransactionRequestMetadata,
                                         CancellationToken cancellationToken);

        /// <summary>
        /// Records the additional response data.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="additionalTransactionResponseMetadata">The additional transaction response metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task RecordAdditionalResponseData(Guid estateId,
                                          Guid transactionId,
                                          String operatorIdentifier,
                                          Dictionary<String, String> additionalTransactionResponseMetadata,
                                          CancellationToken cancellationToken);

        /// <summary>
        /// Requests the email receipt.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="customerEmailAddress">The customer email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task RequestEmailReceipt(Guid estateId,
                                 Guid transactionId,
                                 String customerEmailAddress,
                                 CancellationToken cancellationToken);

        /// <summary>
        /// Starts the transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
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