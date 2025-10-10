using System.Linq;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.DataTransferObjects.Responses.Operator;
using ProductType = TransactionProcessor.DataTransferObjects.Responses.Contract.ProductType;

namespace TransactionProcessor.Factories
{
    using System;
    using System.Collections.Generic;
    using DataTransferObjects;
    using Models;
    using Newtonsoft.Json;
    using TransactionProcessor.Models.Estate;
    using TransactionProcessor.Models.Merchant;
    using CalculationType = DataTransferObjects.Responses.Contract.CalculationType;
    using IssueVoucherResponse = DataTransferObjects.IssueVoucherResponse;
    using RedeemVoucherResponse = DataTransferObjects.RedeemVoucherResponse;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.Factories.IModelFactory" />
    public static class ModelFactory
    {
        #region Methods
        public static Result<List<ContractResponse>> ConvertFrom(List<Models.Contract.Contract> contracts)
        {
            List<Result<ContractResponse>> result = new();

            contracts.ForEach(c => result.Add(ModelFactory.ConvertFrom(c)));

            if (result.Any(c => c.IsFailed))
                return Result.Failure("Failed converting contracts");

            return Result.Success(result.Select(r => r.Data).ToList());
        }

        public static Result<ContractResponse> ConvertFrom(Models.Contract.Contract contract)
        {
            if (contract == null)
            {
                return Result.Invalid("contract cannot be null");
            }

            ContractResponse contractResponse = new ContractResponse
            {
                EstateId = contract.EstateId,
                EstateReportingId = contract.EstateReportingId,
                ContractId = contract.ContractId,
                ContractReportingId = contract.ContractReportingId,
                OperatorId = contract.OperatorId,
                OperatorName = contract.OperatorName,
                Description = contract.Description
            };

            if (contract.Products != null && contract.Products.Any())
            {
                contractResponse.Products = new List<DataTransferObjects.Responses.Contract.ContractProduct>();

                contract.Products.ForEach(p => {
                    DataTransferObjects.Responses.Contract.ContractProduct contractProduct = new DataTransferObjects.Responses.Contract.ContractProduct
                    {
                        ProductReportingId = p.ContractProductReportingId,
                        ProductId = p.ContractProductId,
                        Value = p.Value,
                        DisplayText = p.DisplayText,
                        Name = p.Name,
                        ProductType = Enum.Parse<ProductType>(p.ProductType.ToString())
                    };
                    if (p.TransactionFees != null && p.TransactionFees.Any())
                    {
                        contractProduct.TransactionFees = new List<DataTransferObjects.Responses.Contract.ContractProductTransactionFee>();
                        p.TransactionFees.ForEach(tf => {
                            DataTransferObjects.Responses.Contract.ContractProductTransactionFee transactionFee = new DataTransferObjects.Responses.Contract.ContractProductTransactionFee
                            {
                                TransactionFeeId = tf.TransactionFeeId,
                                Value = tf.Value,
                                Description = tf.Description,
                            };
                            transactionFee.CalculationType =
                                Enum.Parse<CalculationType>(tf.CalculationType.ToString());

                            contractProduct.TransactionFees.Add(transactionFee);
                        });
                    }

                    contractResponse.Products.Add(contractProduct);
                });
            }

            return Result.Success(contractResponse);
        }

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

        public static Result<List<DataTransferObjects.Responses.Contract.ContractProductTransactionFee>> ConvertFrom(List<Models.Contract.ContractProductTransactionFee> transactionFees)
        {
            List<DataTransferObjects.Responses.Contract.ContractProductTransactionFee> result = new();
            transactionFees.ForEach(tf => {
                DataTransferObjects.Responses.Contract.ContractProductTransactionFee transactionFee = new DataTransferObjects.Responses.Contract.ContractProductTransactionFee
                {
                    TransactionFeeId = tf.TransactionFeeId,
                    Value = tf.Value,
                    Description = tf.Description,
                };
                transactionFee.CalculationType = Enum.Parse<CalculationType>(tf.CalculationType.ToString());
                transactionFee.FeeType = Enum.Parse<DataTransferObjects.Responses.Contract.FeeType>(tf.FeeType.ToString());

                result.Add(transactionFee);
            });

            return Result.Success(result);
        }

        public static Result<List<MerchantResponse>> ConvertFrom(List<Models.Merchant.Merchant> merchants)
        {
            List<Result<MerchantResponse>> result = new();

            if (merchants == null)
                return Result.Success(new List<MerchantResponse>());

            merchants.ForEach(c => result.Add(ModelFactory.ConvertFrom(c)));

            if (result.Any(c => c.IsFailed))
                return Result.Failure("Failed converting merchants");

            return Result.Success(result.Select(r => r.Data).ToList());
        }

        private static TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule ConvertFrom(Models.Merchant.SettlementSchedule settlementSchedule)
        {
            return settlementSchedule switch
            {
                Models.Merchant.SettlementSchedule.Weekly => TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule.Weekly,
                Models.Merchant.SettlementSchedule.Monthly => TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule.Monthly,
                Models.Merchant.SettlementSchedule.Immediate => TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule.Immediate,
                Models.Merchant.SettlementSchedule.NotSet => TransactionProcessor.DataTransferObjects.Responses.Merchant.SettlementSchedule.NotSet,
            };
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

        public static Result<MerchantResponse> ConvertFrom(Models.Merchant.Merchant merchant)
        {
            if (merchant == null)
            {
                return Result.Invalid("merchant cannot be null");
            }

            MerchantResponse merchantResponse = new MerchantResponse
            {
                EstateId = merchant.EstateId,
                EstateReportingId = merchant.EstateReportingId,
                MerchantId = merchant.MerchantId,
                MerchantReportingId = merchant.MerchantReportingId,
                MerchantName = merchant.MerchantName,
                SettlementSchedule = ModelFactory.ConvertFrom(merchant.SettlementSchedule),
                MerchantReference = merchant.Reference,
                NextStatementDate = merchant.NextStatementDate
            };

            if (merchant.Addresses != null && merchant.Addresses.Any())
            {
                merchantResponse.Addresses = new List<AddressResponse>();

                merchant.Addresses.ForEach(a => merchantResponse.Addresses.Add(new AddressResponse
                {
                    AddressId = a.AddressId,
                    Town = a.Town,
                    Region = a.Region,
                    PostalCode = a.PostalCode,
                    Country = a.Country,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    AddressLine3 = a.AddressLine3,
                    AddressLine4 = a.AddressLine4
                }));
            }

            if (merchant.Contacts != null && merchant.Contacts.Any())
            {
                merchantResponse.Contacts = new List<ContactResponse>();

                merchant.Contacts.ForEach(c => merchantResponse.Contacts.Add(new ContactResponse
                {
                    ContactId = c.ContactId,
                    ContactPhoneNumber = c.ContactPhoneNumber,
                    ContactEmailAddress = c.ContactEmailAddress,
                    ContactName = c.ContactName
                }));
            }

            if (merchant.Devices != null && merchant.Devices.Any())
            {
                merchantResponse.Devices = new Dictionary<Guid, String>();

                foreach (Device device in merchant.Devices)
                {
                    merchantResponse.Devices.Add(device.DeviceId, device.DeviceIdentifier);
                }
            }

            if (merchant.Operators != null && merchant.Operators.Any())
            {
                merchantResponse.Operators = new List<MerchantOperatorResponse>();

                merchant.Operators.ForEach(a => merchantResponse.Operators.Add(new MerchantOperatorResponse
                {
                    Name = a.Name,
                    MerchantNumber = a.MerchantNumber,
                    OperatorId = a.OperatorId,
                    TerminalNumber = a.TerminalNumber,
                    IsDeleted = a.IsDeleted
                }));
            }

            if (merchant.Contracts != null && merchant.Contracts.Any())
            {
                merchantResponse.Contracts = new List<MerchantContractResponse>();
                merchant.Contracts.ForEach(mc => {
                    merchantResponse.Contracts.Add(new MerchantContractResponse()
                    {
                        ContractId = mc.ContractId,
                        ContractProducts = mc.ContractProducts,
                        IsDeleted = mc.IsDeleted,
                    });
                });
            }

            return merchantResponse;
        }
    }
}