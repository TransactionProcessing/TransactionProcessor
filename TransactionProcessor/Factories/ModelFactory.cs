namespace TransactionProcessor.Factories
{
    using System;
    using System.Collections.Generic;
    using BusinessLogic.Requests;
    using DataTransferObjects;
    using Models;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.Factories.IModelFactory" />
    public class ModelFactory : IModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="processLogonTransactionResponse">The process logon transaction response.</param>
        /// <returns></returns>
        public SerialisedMessage ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse)
        {
            if (processLogonTransactionResponse == null)
            {
                return null;
            }

            LogonTransactionResponse logonTransactionResponse = new LogonTransactionResponse
                                                                {
                                                                    ResponseMessage = processLogonTransactionResponse.ResponseMessage,
                                                                    ResponseCode = processLogonTransactionResponse.ResponseCode,
                                                                    MerchantId = processLogonTransactionResponse.MerchantId,
                                                                    EstateId = processLogonTransactionResponse.EstateId,
                                                                    TransactionId = processLogonTransactionResponse.TransactionId
                                                                };

            return new SerialisedMessage
                   {
                       Metadata = new Dictionary<String, String>()
                                  {
                                      {MetadataContants.KeyNameEstateId, logonTransactionResponse.EstateId.ToString()},
                                      {MetadataContants.KeyNameMerchantId, logonTransactionResponse.MerchantId.ToString()}
                                  },
                       SerialisedData = JsonConvert.SerializeObject(logonTransactionResponse, new JsonSerializerSettings
                                                                                                     {
                                                                                                         TypeNameHandling = TypeNameHandling.All
                                                                                                     })
                   };
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="processSaleTransactionResponse">The process sale transaction response.</param>
        /// <returns></returns>
        public SerialisedMessage ConvertFrom(ProcessSaleTransactionResponse processSaleTransactionResponse)
        {
            if (processSaleTransactionResponse == null)
            {
                return null;
            }

            SaleTransactionResponse saleTransactionResponse = new SaleTransactionResponse
                                                                {
                                                                    ResponseMessage = processSaleTransactionResponse.ResponseMessage,
                                                                    ResponseCode = processSaleTransactionResponse.ResponseCode,
                                                                    MerchantId = processSaleTransactionResponse.MerchantId,
                                                                    EstateId = processSaleTransactionResponse.EstateId,
                                                                    AdditionalTransactionMetadata = processSaleTransactionResponse.AdditionalTransactionMetadata,
                                                                    TransactionId = processSaleTransactionResponse.TransactionId
                                                                };

            return new SerialisedMessage
                   {
                       Metadata = new Dictionary<String, String>()
                                  {
                                      {MetadataContants.KeyNameEstateId, processSaleTransactionResponse.EstateId.ToString()},
                                      {MetadataContants.KeyNameMerchantId, processSaleTransactionResponse.MerchantId.ToString()}
                                  },
                       SerialisedData = JsonConvert.SerializeObject(saleTransactionResponse, new JsonSerializerSettings
                                                                                             {
                                                                                                 TypeNameHandling = TypeNameHandling.All
                                                                                             })
                   };
        }


        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="processReconciliationTransactionResponse">The process reconciliation transaction response.</param>
        /// <returns></returns>
        public SerialisedMessage ConvertFrom(ProcessReconciliationTransactionResponse processReconciliationTransactionResponse)
        {
            if (processReconciliationTransactionResponse == null)
            {
                return null;
            }

            ReconciliationResponse reconciliationTransactionResponse = new ReconciliationResponse
            {
                                                                           ResponseMessage = processReconciliationTransactionResponse.ResponseMessage,
                                                                           ResponseCode = processReconciliationTransactionResponse.ResponseCode,
                                                                           MerchantId = processReconciliationTransactionResponse.MerchantId,
                                                                           EstateId = processReconciliationTransactionResponse.EstateId,
                                                                           TransactionId = processReconciliationTransactionResponse.TransactionId
                                                                       };

            return new SerialisedMessage
                   {
                       Metadata = new Dictionary<String, String>()
                                  {
                                      {MetadataContants.KeyNameEstateId, processReconciliationTransactionResponse.EstateId.ToString()},
                                      {MetadataContants.KeyNameMerchantId, processReconciliationTransactionResponse.MerchantId.ToString()}
                                  },
                       SerialisedData = JsonConvert.SerializeObject(reconciliationTransactionResponse, new JsonSerializerSettings
                                                                                                       {
                                                                                                           TypeNameHandling = TypeNameHandling.All
                                                                                                       })
                   };
        }

        #endregion
    }
}