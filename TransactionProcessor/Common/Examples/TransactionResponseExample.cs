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

    public class GetVoucherResponseExample : IExamplesProvider<GetVoucherResponse>
    {
        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public GetVoucherResponse GetExamples()
        {
            return new GetVoucherResponse
                   {
                       VoucherCode = ExampleData.VoucherCode,
                       Value = ExampleData.VoucherValue,
                       ExpiryDate = ExampleData.ExpiryDate,
                       Balance = ExampleData.RemainingBalance,
                       GeneratedDateTime = ExampleData.GeneratedDateTime.Value,
                       IsGenerated = ExampleData.IsGenerated,
                       IsIssued = ExampleData.IsIssued,
                       IsRedeemed = ExampleData.IsRedeemed,
                       IssuedDateTime = ExampleData.IssuedDateTime.Value,
                       RedeemedDateTime = ExampleData.RedeemedDateTime.Value,
                       TransactionId = ExampleData.TransactionId,
                       VoucherId = ExampleData.VoucherId
                   };
        }
    }

    public class IssueVoucherRequestExample : IMultipleExamplesProvider<IssueVoucherRequest>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SwaggerExample<IssueVoucherRequest>> GetExamples()
        {
            SwaggerExample<IssueVoucherRequest> voucherIssueToEmail = new SwaggerExample<IssueVoucherRequest>
            {
                Name = "Issue Voucher to Email Address",
                Value = new IssueVoucherRequest
                {
                    EstateId = ExampleData.EstateId,
                    IssuedDateTime = ExampleData.IssuedDateTime,
                    OperatorIdentifier = ExampleData.OperatorIdentifier,
                    RecipientEmail = ExampleData.RecipientEmail,
                    TransactionId = ExampleData.TransactionId,
                    Value = ExampleData.VoucherValue
                }
            };

            SwaggerExample<IssueVoucherRequest> voucherIssueToMobile = new SwaggerExample<IssueVoucherRequest>
            {
                Name = "Issue Voucher to Mobile Number",
                Value = new IssueVoucherRequest
                {
                    EstateId = ExampleData.EstateId,
                    IssuedDateTime = ExampleData.IssuedDateTime,
                    OperatorIdentifier = ExampleData.OperatorIdentifier,
                    RecipientMobile = ExampleData.RecipientMobile,
                    TransactionId = ExampleData.TransactionId,
                    Value = ExampleData.VoucherValue
                }
            };

            return new List<SwaggerExample<IssueVoucherRequest>>
                   {
                       voucherIssueToEmail,
                       voucherIssueToMobile
                   };
        }

        #endregion
    }

    public class IssueVoucherResponseExample : IExamplesProvider<IssueVoucherResponse>
    {
        #region Methods

        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public IssueVoucherResponse GetExamples()
        {
            return new IssueVoucherResponse
                   {
                       ExpiryDate = ExampleData.ExpiryDate,
                       Message = ExampleData.Message,
                       VoucherCode = ExampleData.VoucherCode,
                       VoucherId = ExampleData.VoucherId
                   };
        }

        #endregion
    }

    public class RedeemVoucherRequestExample : IExamplesProvider<RedeemVoucherRequest>
    {
        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public RedeemVoucherRequest GetExamples()
        {
            return new RedeemVoucherRequest
                   {
                       EstateId = ExampleData.EstateId,
                       RedeemedDateTime = ExampleData.RedeemedDateTime,
                       VoucherCode = ExampleData.VoucherCode
                   };
        }
    }

    public class RedeemVoucherResponseExample : IExamplesProvider<RedeemVoucherResponse>
    {
        /// <summary>
        /// Gets the examples.
        /// </summary>
        /// <returns></returns>
        public RedeemVoucherResponse GetExamples()
        {
            return new RedeemVoucherResponse
                   {
                       VoucherCode = ExampleData.VoucherCode,
                       ExpiryDate = ExampleData.ExpiryDate,
                       RemainingBalance = ExampleData.RemainingBalance
                   };
        }
    }
}