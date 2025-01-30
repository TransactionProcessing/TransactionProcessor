using System.Linq;
using EstateManagement.DataTransferObjects.Responses.Contract;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Responses.Operator;

namespace TransactionProcessor.Factories
{
    using System;
    using System.Collections.Generic;
    using BusinessLogic.Requests;
    using DataTransferObjects;
    using EstateManagement.DataTransferObjects.Responses.Estate;
    using Models;
    using Newtonsoft.Json;
    using TransactionProcessor.Models.Estate;
    using IssueVoucherResponse = DataTransferObjects.IssueVoucherResponse;
    using RedeemVoucherResponse = DataTransferObjects.RedeemVoucherResponse;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.Factories.IModelFactory" />
    public static class ModelFactory
    {
        #region Methods

        public static Result<OperatorResponse> ConvertFrom(Models.Operator.Operator @operator)
        {
            if (@operator == null)
            {
                return Result.Invalid("operator cannot be null");
            }

            OperatorResponse response = new();
            response.OperatorId = @operator.OperatorId;
            response.RequireCustomTerminalNumber = @operator.RequireCustomTerminalNumber;
            response.RequireCustomMerchantNumber = @operator.RequireCustomMerchantNumber;
            response.Name = @operator.Name;

            return Result.Success(response);
        }

        public static Result<List<OperatorResponse>> ConvertFrom(List<Models.Operator.Operator> @operators)
        {
            if (@operators == null || @operators.Any() == false)
            {
                return Result.Success(new List<OperatorResponse>());
            }

            List<Result<OperatorResponse>> result = new();

            @operators.ForEach(c => result.Add(ModelFactory.ConvertFrom(c)));

            if (result.Any(c => c.IsFailed))
                return Result.Failure("Failed converting operators");

            return Result.Success(result.Select(r => r.Data).ToList());
        }

        public static Result<SerialisedMessage> ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse)
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

        public static Result<SerialisedMessage> ConvertFrom(ProcessSaleTransactionResponse processSaleTransactionResponse)
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

        public static Result<SerialisedMessage> ConvertFrom(ProcessReconciliationTransactionResponse processReconciliationTransactionResponse)
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

        public static IssueVoucherResponse ConvertFrom(Models.IssueVoucherResponse issueVoucherResponse)
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
        public static Result<GetVoucherResponse> ConvertFrom(Voucher voucherModel)
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
        public static Result<RedeemVoucherResponse> ConvertFrom(Models.RedeemVoucherResponse redeemVoucherResponse)
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

        public static Result<List<DataTransferObjects.Responses.Estate.EstateResponse>> ConvertFrom(List<Estate> estates)
        {
            List<Result<DataTransferObjects.Responses.Estate.EstateResponse>> result = new();

            estates.ForEach(c => result.Add(ModelFactory.ConvertFrom(c)));

            if (result.Any(c => c.IsFailed))
                return Result.Failure("Failed converting estates");

            return Result.Success(result.Select(r => r.Data).ToList());

        }

        public static Result<DataTransferObjects.Responses.Estate.EstateResponse> ConvertFrom(Estate estate)
        {
            if (estate == null)
            {
                return Result.Invalid("estate cannot be null");
            }

            DataTransferObjects.Responses.Estate.EstateResponse estateResponse = new()
            {
                EstateName = estate.Name,
                EstateId = estate.EstateId,
                EstateReportingId = estate.EstateReportingId,
                EstateReference = estate.Reference,
                Operators = new List<DataTransferObjects.Responses.Estate.EstateOperatorResponse>(),
                SecurityUsers = new List<DataTransferObjects.Responses.Estate.SecurityUserResponse>()
            };

            if (estate.Operators != null && estate.Operators.Any())
            {
                estate.Operators.ForEach(o => estateResponse.Operators.Add(new DataTransferObjects.Responses.Estate.EstateOperatorResponse
                {
                    OperatorId = o.OperatorId,
                    //RequireCustomTerminalNumber = o.RequireCustomTerminalNumber,
                    //RequireCustomMerchantNumber = o.RequireCustomMerchantNumber,
                    Name = o.Name,
                    IsDeleted = o.IsDeleted
                }));
            }

            if (estate.SecurityUsers != null && estate.SecurityUsers.Any())
            {
                estate.SecurityUsers.ForEach(s => estateResponse.SecurityUsers.Add(new DataTransferObjects.Responses.Estate.SecurityUserResponse
                {
                    EmailAddress = s.EmailAddress,
                    SecurityUserId = s.SecurityUserId
                }));
            }

            return Result.Success(estateResponse);
        }

        #endregion

        public static Object ConvertFrom(ContractResponse processReconciliationTransactionResponse) {
            throw new NotImplementedException();
        }
    }
}