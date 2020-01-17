namespace TransactionProcessor.BusinessLogic.Services
{
    using System;
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
        Task<ProcessLogonTransactionResponse> ProcessLogonTransaction(Guid transactionId, Guid estateId, Guid merchantId, DateTime transactionDateTime,
                                                                      String transactionNumber, String deviceIdentifier, CancellationToken cancellationToken);

        #endregion
    }
}