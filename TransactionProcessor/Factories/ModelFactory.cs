namespace TransactionProcessor.Factories
{
    using System;
    using System.Collections.Generic;
    using BusinessLogic.Requests;
    using DataTransferObjects;
    using Models;
    using Newtonsoft.Json;
    using IssueVoucherResponse = DataTransferObjects.IssueVoucherResponse;
    using RedeemVoucherResponse = DataTransferObjects.RedeemVoucherResponse;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.Factories.IModelFactory" />
    public class ModelFactory : IModelFactory
    {
        #region Methods

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

        public IssueVoucherResponse ConvertFrom(Models.IssueVoucherResponse issueVoucherResponse)
        {
            if (issueVoucherResponse == null)
            {
                return null;
            }

            IssueVoucherResponse response = new IssueVoucherResponse
            {
                Message = issueVoucherResponse.Message,
                ExpiryDate = issueVoucherResponse.ExpiryDate,
                VoucherCode = issueVoucherResponse.VoucherCode,
                VoucherId = issueVoucherResponse.VoucherId
            };

            return response;
        }
        public GetVoucherResponse ConvertFrom(Voucher voucherModel)
        {
            if (voucherModel == null)
            {
                return null;
            }

            GetVoucherResponse response = new GetVoucherResponse
            {
                Value = voucherModel.Value,
                Balance = voucherModel.Balance,
                ExpiryDate = voucherModel.ExpiryDate,
                VoucherCode = voucherModel.VoucherCode,
                TransactionId = voucherModel.TransactionId,
                IssuedDateTime = voucherModel.IssuedDateTime,
                IsIssued = voucherModel.IsIssued,
                GeneratedDateTime = voucherModel.GeneratedDateTime,
                IsGenerated = voucherModel.IsGenerated,
                IsRedeemed = voucherModel.IsRedeemed,
                RedeemedDateTime = voucherModel.RedeemedDateTime,
                VoucherId = voucherModel.VoucherId
            };

            return response;
        }
        public RedeemVoucherResponse ConvertFrom(Models.RedeemVoucherResponse redeemVoucherResponse)
        {
            if (redeemVoucherResponse == null)
            {
                return null;
            }

            RedeemVoucherResponse response = new RedeemVoucherResponse
            {
                ExpiryDate = redeemVoucherResponse.ExpiryDate,
                VoucherCode = redeemVoucherResponse.VoucherCode,
                RemainingBalance = redeemVoucherResponse.RemainingBalance
            };

            return response;
        }

        #endregion
    }
}