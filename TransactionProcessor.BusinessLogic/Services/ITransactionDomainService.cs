namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

    /// <summary>
    /// 
    /// </summary>
    public interface ITransactionDomainService
    {
        #region Methods

        /// <summary>
        /// Processes the logon transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ProcessLogonTransactionResponse> ProcessLogonTransaction(Guid transactionId,
                                                                      Guid estateId,
                                                                      Guid merchantId,
                                                                      DateTime transactionDateTime,
                                                                      String transactionNumber,
                                                                      String deviceIdentifier,
                                                                      CancellationToken cancellationToken);

        /// <summary>
        /// Processes the sale transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="operatorId">The operator identifier.</param>
        /// <param name="customerEmailAddress">The customer email address.</param>
        /// <param name="additionalTransactionMetadata">The additional transaction metadata.</param>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ProcessSaleTransactionResponse> ProcessSaleTransaction(Guid transactionId,
                                                                    Guid estateId,
                                                                    Guid merchantId,
                                                                    DateTime transactionDateTime,
                                                                    String transactionNumber,
                                                                    String deviceIdentifier,
                                                                    String operatorId,
                                                                    String customerEmailAddress,
                                                                    Dictionary<String, String> additionalTransactionMetadata,
                                                                    Guid contractId,
                                                                    Guid productId,
                                                                    CancellationToken cancellationToken);

        /// <summary>
        /// Processes the reconciliation transaction.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionCount">The transaction count.</param>
        /// <param name="transactionValue">The transaction value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<ProcessReconciliationTransactionResponse> ProcessReconciliationTransaction(Guid transactionId,
                                                                                        Guid estateId,
                                                                                        Guid merchantId,
                                                                                        String deviceIdentifier,
                                                                                        DateTime transactionDateTime,
                                                                                        Int32 transactionCount,
                                                                                        Decimal transactionValue,
                                                                                        CancellationToken cancellationToken);

        Task<ProcessSettlementResponse> ProcessSettlement(DateTime pendingSettlementDate,
                                                          Guid estateId,
                                                          CancellationToken cancellationToken);

        #endregion
    }
}