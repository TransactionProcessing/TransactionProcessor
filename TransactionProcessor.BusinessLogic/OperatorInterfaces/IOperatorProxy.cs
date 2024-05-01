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
        Task<OperatorResponse> ProcessLogonMessage(String accessToken, 
                                                   CancellationToken cancellationToken);

        Task<OperatorResponse> ProcessSaleMessage(String accessToken,
                                                  Guid transactionId,
                                                  Guid operatorId,
                                                  EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse merchant,
                                                  DateTime transactionDateTime,
                                                  String transactionReference,
                                                  Dictionary<String, String> additionalTransactionMetadata,
                                                  CancellationToken cancellationToken);

        #endregion
    }
}