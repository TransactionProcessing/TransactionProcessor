using SimpleResults;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EstateManagement.DataTransferObjects.Responses.Merchant;

    /// <summary>
    /// 
    /// </summary>
    public interface IOperatorProxy
    {
        #region Methods
        Task<Result<OperatorResponse>> ProcessLogonMessage(String accessToken, 
                                                           CancellationToken cancellationToken);

        Task<Result<OperatorResponse>> ProcessSaleMessage(String accessToken,
                                                          Guid transactionId,
                                                          Guid operatorId,
                                                          MerchantResponse merchant,
                                                          DateTime transactionDateTime,
                                                          String transactionReference,
                                                          Dictionary<String, String> additionalTransactionMetadata,
                                                          CancellationToken cancellationToken);

        #endregion
    }
}