namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Models;

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
        
        Task<ProcessSaleTransactionResponse> ProcessSaleTransaction(Guid transactionId,
                                                                    Guid estateId,
                                                                    Guid merchantId,
                                                                    DateTime transactionDateTime,
                                                                    String transactionNumber,
                                                                    String deviceIdentifier,
                                                                    Guid operatorId,
                                                                    String customerEmailAddress,
                                                                    Dictionary<String, String> additionalTransactionMetadata,
                                                                    Guid contractId,
                                                                    Guid productId,
                                                                    Int32 transactionSource,
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

        Task ResendTransactionReceipt(Guid transactionId,
                                      Guid estateId,
                                      CancellationToken cancellationToken);

        #endregion
    }
}