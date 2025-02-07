using SimpleResults;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public interface IOperatorProxy
    {
        #region Methods
        Task<Result<OperatorResponse>> ProcessLogonMessage(CancellationToken cancellationToken);

        Task<Result<OperatorResponse>> ProcessSaleMessage(Guid transactionId,
                                                          Guid operatorId,
                                                          Models.Merchant.Merchant merchant,
                                                          DateTime transactionDateTime,
                                                          String transactionReference,
                                                          Dictionary<String, String> additionalTransactionMetadata,
                                                          CancellationToken cancellationToken);

        #endregion
    }
}