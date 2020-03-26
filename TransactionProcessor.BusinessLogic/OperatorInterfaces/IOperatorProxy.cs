namespace TransactionProcessor.BusinessLogic.OperatorInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EstateManagement.DataTransferObjects.Responses;

    /// <summary>
    /// 
    /// </summary>
    public interface IOperatorProxy
    {
        #region Methods

        /// <summary>
        /// Processes the sale message.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="merchant">The merchant.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="additionalTransactionMetadata">The additional transaction metadata.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<OperatorResponse> ProcessSaleMessage(Guid transactionId,
                                                  MerchantResponse merchant,
                                                  DateTime transactionDateTime,
                                                  String transactionReference,
                                                  Dictionary<String, String> additionalTransactionMetadata,
                                                  CancellationToken cancellationToken);

        #endregion
    }
}