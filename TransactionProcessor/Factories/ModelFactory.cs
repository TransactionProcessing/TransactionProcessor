using SimpleResults;

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

        public Result<SerialisedMessage> ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse)
        {
            if (processLogonTransactionResponse == null)
            {
                return Result.Invalid("processLogonTransactionResponse cannot be null");
            }

            LogonTransactionResponse logonTransactionResponse = new LogonTransactionResponse
                                                                {
                                                                    ResponseMessage = processLogonTransactionResponse.ResponseMessage,
                                                                    ResponseCode = processLogonTransactionResponse.ResponseCode,
                                                                    MerchantId = processLogonTransactionResponse.MerchantId,
                                                                    EstateId = processLogonTransactionResponse.EstateId,
                                                                    TransactionId = processLogonTransactionResponse.TransactionId
                                                                };

            return Result.Success(new SerialisedMessage
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
                   });
        }

        public Result<SerialisedMessage> ConvertFrom(ProcessSaleTransactionResponse processSaleTransactionResponse)
        {
            if (processSaleTransactionResponse == null)
            {
                return Result.Invalid("processSaleTransactionResponse cannot be null");
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

            return Result.Success(new SerialisedMessage
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
                   });
        }

        public Result<SerialisedMessage> ConvertFrom(ProcessReconciliationTransactionResponse processReconciliationTransactionResponse)
        {
            if (processReconciliationTransactionResponse == null)
            {
                return Result.Invalid("processReconciliationTransactionResponse cannot be null");
            }

            ReconciliationResponse reconciliationTransactionResponse = new ReconciliationResponse
            {
                                                                           ResponseMessage = processReconciliationTransactionResponse.ResponseMessage,
                                                                           ResponseCode = processReconciliationTransactionResponse.ResponseCode,
                                                                           MerchantId = processReconciliationTransactionResponse.MerchantId,
                                                                           EstateId = processReconciliationTransactionResponse.EstateId,
                                                                           TransactionId = processReconciliationTransactionResponse.TransactionId
                                                                       };

            return Result.Success(new SerialisedMessage
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
                   });
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
        public Result<GetVoucherResponse> ConvertFrom(Voucher voucherModel)
        {
            if (voucherModel == null)
            {
                return Result.Invalid("voucherModel cannot be null");
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

            return Result.Success(response);
        }
        public Result<RedeemVoucherResponse> ConvertFrom(Models.RedeemVoucherResponse redeemVoucherResponse)
        {
            if (redeemVoucherResponse == null)
            {
                return Result.Invalid("redeemVoucherResponse cannot be null");
            }

            RedeemVoucherResponse response = new RedeemVoucherResponse
            {
                ExpiryDate = redeemVoucherResponse.ExpiryDate,
                VoucherCode = redeemVoucherResponse.VoucherCode,
                RemainingBalance = redeemVoucherResponse.RemainingBalance
            };

            return Result.Success(response);
        }

        #endregion
    }
}