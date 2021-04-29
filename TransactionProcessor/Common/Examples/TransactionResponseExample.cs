namespace TransactionProcessor.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IMultipleExamplesProvider{TransactionProcessor.DataTransferObjects.SerialisedMessage}" />
    public class TransactionResponseExample : IMultipleExamplesProvider<SerialisedMessage>
    {
        /// <summary>
        /// Gets the logon example.
        /// </summary>
        /// <returns></returns>
        private SwaggerExample<SerialisedMessage> GetLogonExample()
        {
            LogonTransactionResponse response = new LogonTransactionResponse
                                                {
                                                    EstateId = ExampleData.EstateId,
                                                    MerchantId = ExampleData.MerchantId,
                                                    ResponseCode = ExampleData.LogonResponseCode,
                                                    ResponseMessage = ExampleData.LogonResponseMessage,
                                                    TransactionId = ExampleData.TransactionId
                                                };

            return new SwaggerExample<SerialisedMessage>
                   {
                       Name = "Logon Response",
                       Value = new SerialisedMessage
                               {
                                   Metadata = new Dictionary<String, String>
                                              {
                                                  {ExampleData.EstateIdMetadataName, ExampleData.EstateId.ToString()},
                                                  {ExampleData.MerchantIdMetadataName, ExampleData.MerchantId.ToString()}
                                              },
                                   SerialisedData = JsonConvert.SerializeObject(response, Formatting.Indented)
                               }
                   };
        }

        /// <summary>
        /// Gets the reconciliation example.
        /// </summary>
        /// <returns></returns>
        private SwaggerExample<SerialisedMessage> GetReconciliationExample()
        {
            ReconciliationResponse response = new ReconciliationResponse
                                              {
                                                  EstateId = ExampleData.EstateId,
                                                  MerchantId = ExampleData.MerchantId,
                                                  ResponseCode = ExampleData.ReconciliationResponseCode,
                                                  ResponseMessage = ExampleData.ReconciliationResponseMessage,
                                                  TransactionId = ExampleData.TransactionId
            };

            return new SwaggerExample<SerialisedMessage>
                   {
                       Name = "Reconciliation Response",
                       Value = new SerialisedMessage
                               {
                                   Metadata = new Dictionary<String, String>
                                              {
                                                  {ExampleData.EstateIdMetadataName, ExampleData.EstateId.ToString()},
                                                  {ExampleData.MerchantIdMetadataName, ExampleData.MerchantId.ToString()}
                                              },
                                   SerialisedData = JsonConvert.SerializeObject(response, Formatting.Indented)
                               }
                   };
        }

        /// <summary>
        /// Gets the sale example.
        /// </summary>
        /// <returns></returns>
        private SwaggerExample<SerialisedMessage> GetSaleExample()
        {
            SaleTransactionResponse response = new SaleTransactionResponse
                                               {
                                                   EstateId = ExampleData.EstateId,
                                                   MerchantId = ExampleData.MerchantId,
                                                   AdditionalTransactionMetadata = new Dictionary<String, String>(),
                                                   ResponseCode = ExampleData.SaleResponseCode,
                                                   ResponseMessage = ExampleData.SaleResponseMessage,
                                                   TransactionId = ExampleData.TransactionId
            };

            return new SwaggerExample<SerialisedMessage>
                   {
                       Name = "Sale Response",
                       Value = new SerialisedMessage
                               {
                                   Metadata = new Dictionary<String, String>
                                              {
                                                  {ExampleData.EstateIdMetadataName, ExampleData.EstateId.ToString()},
                                                  {ExampleData.MerchantIdMetadataName, ExampleData.MerchantId.ToString()}
                                              },
                                   SerialisedData = JsonConvert.SerializeObject(response, Formatting.Indented)
                               }
                   };
        }

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SwaggerExample<SerialisedMessage>> GetExamples()
        {
            return new List<SwaggerExample<SerialisedMessage>>
                   {
                       this.GetLogonExample(),
                       this.GetSaleExample(),
                       this.GetReconciliationExample()
                   };
        }
    }
}