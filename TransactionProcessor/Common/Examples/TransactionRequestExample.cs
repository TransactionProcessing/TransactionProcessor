namespace TransactionProcessor.Common.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using DataTransferObjects;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.Filters.IMultipleExamplesProvider{TransactionProcessor.DataTransferObjects.SerialisedMessage}" />
    [ExcludeFromCodeCoverage]
    public class TransactionRequestExample : IMultipleExamplesProvider<SerialisedMessage>
    {
        #region Methods

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

        /// <summary>
        /// Gets the logon example.
        /// </summary>
        /// <returns></returns>
        private SwaggerExample<SerialisedMessage> GetLogonExample()
        {
            LogonTransactionRequest request = new LogonTransactionRequest
                                              {
                                                  DeviceIdentifier = ExampleData.DeviceIdentifier,
                                                  EstateId = ExampleData.EstateId,
                                                  MerchantId = ExampleData.MerchantId,
                                                  TransactionDateTime = ExampleData.TransactionDateTime,
                                                  TransactionNumber = ExampleData.TransactionNumber,
                                                  TransactionType = ExampleData.TransactionTypeLogon
                                              };

            return new SwaggerExample<SerialisedMessage>
                   {
                       Name = "Logon Request",
                       Value = new SerialisedMessage
                               {
                                   Metadata = new Dictionary<String, String>
                                              {
                                                  {ExampleData.EstateIdMetadataName, ExampleData.EstateId.ToString()},
                                                  {ExampleData.MerchantIdMetadataName, ExampleData.MerchantId.ToString()}
                                              },
                                   SerialisedData = JsonConvert.SerializeObject(request, Formatting.Indented)
                               }
                   };
        }

        /// <summary>
        /// Gets the reconciliation example.
        /// </summary>
        /// <returns></returns>
        private SwaggerExample<SerialisedMessage> GetReconciliationExample()
        {
            ReconciliationRequest request = new ReconciliationRequest
                                            {
                                                DeviceIdentifier = ExampleData.DeviceIdentifier,
                                                EstateId = ExampleData.EstateId,
                                                MerchantId = ExampleData.MerchantId,
                                                TransactionDateTime = ExampleData.TransactionDateTime,
                                                OperatorTotals = new List<OperatorTotalRequest>
                                                                 {
                                                                     new OperatorTotalRequest
                                                                     {
                                                                         ContractId = ExampleData.ContractId,
                                                                         OperatorIdentifier = ExampleData.OperatorIdentifier,
                                                                         TransactionCount = ExampleData.TransactionCount,
                                                                         TransactionValue = ExampleData.TransactionValue
                                                                     }
                                                                 },
                                                TransactionCount = ExampleData.TransactionCount,
                                                TransactionValue = ExampleData.TransactionValue
                                            };

            return new SwaggerExample<SerialisedMessage>
                   {
                       Name = "Reconciliation Request",
                       Value = new SerialisedMessage
                               {
                                   Metadata = new Dictionary<String, String>
                                              {
                                                  {ExampleData.EstateIdMetadataName, ExampleData.EstateId.ToString()},
                                                  {ExampleData.MerchantIdMetadataName, ExampleData.MerchantId.ToString()}
                                              },
                                   SerialisedData = JsonConvert.SerializeObject(request, Formatting.Indented)
                               }
                   };
        }

        /// <summary>
        /// Gets the sale example.
        /// </summary>
        /// <returns></returns>
        private SwaggerExample<SerialisedMessage> GetSaleExample()
        {
            SaleTransactionRequest request = new SaleTransactionRequest
                                             {
                                                 DeviceIdentifier = ExampleData.DeviceIdentifier,
                                                 EstateId = ExampleData.EstateId,
                                                 MerchantId = ExampleData.MerchantId,
                                                 TransactionDateTime = ExampleData.TransactionDateTime,
                                                 TransactionNumber = ExampleData.TransactionNumber,
                                                 TransactionType = ExampleData.TransactionTypeSale,
                                                 AdditionalTransactionMetadata = new Dictionary<String, String>(),
                                                 ContractId = ExampleData.ContractId,
                                                 CustomerEmailAddress = ExampleData.CustomerEmailAddress,
                                                 OperatorIdentifier = ExampleData.OperatorIdentifier,
                                                 ProductId = ExampleData.ProductId
                                             };

            return new SwaggerExample<SerialisedMessage>
                   {
                       Name = "Sale Request",
                       Value = new SerialisedMessage
                               {
                                   Metadata = new Dictionary<String, String>
                                              {
                                                  {ExampleData.EstateIdMetadataName, ExampleData.EstateId.ToString()},
                                                  {ExampleData.MerchantIdMetadataName, ExampleData.MerchantId.ToString()}
                                              },
                                   SerialisedData = JsonConvert.SerializeObject(request, Formatting.Indented)
                               }
                   };
        }

        #endregion
    }
}