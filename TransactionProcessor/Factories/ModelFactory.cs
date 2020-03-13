namespace TransactionProcessor.Factories
{
    using System;
    using System.Collections.Generic;
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
                                                                    EstateId = processLogonTransactionResponse.EstateId
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
                                                                    AdditionalTransactionMetadata = processSaleTransactionResponse.AdditionalTransactionMetadata
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

        #endregion
    }
}