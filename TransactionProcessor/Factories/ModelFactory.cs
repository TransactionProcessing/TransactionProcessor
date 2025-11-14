using System.Linq;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
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
        public static List<ContractResponse> ConvertFrom(List<Models.Contract.Contract> contracts)
        {
            List<Result<ContractResponse>> result = new();

            contracts.ForEach(c => result.Add(ModelFactory.ConvertFrom(c)));
            
            return result.Select(r => r.Data).ToList();
        }

        public static ContractResponse ConvertFrom(Models.Contract.Contract contract)
        {
            if (contract == null) {
                return null;
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

            return contractResponse;
        }

        public static OperatorResponse ConvertFrom(Models.Operator.Operator @operator)
        {
            if (@operator == null) {
                return null;
            }

            OperatorResponse response = new();
            response.OperatorId = @operator.OperatorId;
            response.RequireCustomTerminalNumber = @operator.RequireCustomTerminalNumber;
            response.RequireCustomMerchantNumber = @operator.RequireCustomMerchantNumber;
            response.Name = @operator.Name;

            return response;
        }

        public static List<OperatorResponse> ConvertFrom(List<Models.Operator.Operator> @operators)
        {
            if (@operators == null || @operators.Any() == false)
            {
                return new List<OperatorResponse>();
            }

            List<Result<OperatorResponse>> result = new();

            @operators.ForEach(c => result.Add(ModelFactory.ConvertFrom(c)));


            return result.Select(r => r.Data).ToList();
        }

        public static SerialisedMessage ConvertFrom(ProcessLogonTransactionResponse processLogonTransactionResponse)
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

        public static SerialisedMessage ConvertFrom(ProcessSaleTransactionResponse processSaleTransactionResponse)
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

        public static SerialisedMessage ConvertFrom(ProcessReconciliationTransactionResponse processReconciliationTransactionResponse)
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
        public static GetVoucherResponse ConvertFrom(Voucher voucherModel)
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
        public static RedeemVoucherResponse ConvertFrom(Models.RedeemVoucherResponse redeemVoucherResponse)
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

        public static List<EstateResponse> ConvertFrom(List<Estate> estates)
        {
            List<Result<EstateResponse>> result = new();

            estates.ForEach(c => result.Add(ModelFactory.ConvertFrom(c)));

            if (result.Any(c => c.IsFailed))
                return new List<EstateResponse>();

            return result.Select(r => r.Data).ToList();

        }

        public static List<DataTransferObjects.Responses.Contract.ContractProductTransactionFee> ConvertFrom(List<Models.Contract.ContractProductTransactionFee> transactionFees)
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

            return result;
        }

        public static List<MerchantResponse> ConvertFrom(List<Models.Merchant.Merchant> merchants)
        {
            List<Result<MerchantResponse>> result = new();

            if (merchants == null)
                return new List<MerchantResponse>();

            merchants.ForEach(c => result.Add(ModelFactory.ConvertFrom(c)));
            
            return result.Select(r => r.Data).ToList();
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

        public static DataTransferObjects.Responses.Estate.EstateResponse ConvertFrom(Estate estate)
        {
            if (estate == null) {
                return null;
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

            return estateResponse;
        }

        #endregion

        private static List<AddressResponse> ConvertFrom(List<Address> addresses) {

            List<AddressResponse> result = new List<AddressResponse>();
            if (addresses != null && addresses.Any())
            {
                addresses.ForEach(a => result.Add(new AddressResponse
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

            return result;
        }

        private static List<ContactResponse> ConvertFrom(List<Contact> contacts) {
            List<ContactResponse> result = new List<ContactResponse>();

            if (contacts != null && contacts.Any())
            {
                contacts.ForEach(c => result.Add(new ContactResponse
                {
                    ContactId = c.ContactId,
                    ContactPhoneNumber = c.ContactPhoneNumber,
                    ContactEmailAddress = c.ContactEmailAddress,
                    ContactName = c.ContactName
                }));
            }

            return result;
        }

        public static Dictionary<Guid, String> ConvertFrom(List<Device> devices) {
            Dictionary<Guid, String> result = new();

            if (devices != null && devices.Any()) {
                foreach (Device device in devices) {
                    result.Add(device.DeviceId, device.DeviceIdentifier);
                }
            }

            return result;
        }

        private static List<MerchantOperatorResponse> ConvertFrom(List<Models.Merchant.Operator> operators) {
            List<MerchantOperatorResponse> result = new List<MerchantOperatorResponse>();

            if (operators != null && operators.Any())
            {
                operators.ForEach(a => result.Add(new MerchantOperatorResponse
                {
                    Name = a.Name,
                    MerchantNumber = a.MerchantNumber,
                    OperatorId = a.OperatorId,
                    TerminalNumber = a.TerminalNumber,
                    IsDeleted = a.IsDeleted
                }));
            }

            return result;
        }

        private static List<MerchantContractResponse> ConvertFrom(List<Models.Merchant.Contract> contracts) {
            List<MerchantContractResponse> result = new();

            if (contracts != null && contracts.Any())
            {
                contracts.ForEach(mc => {
                    result.Add(new MerchantContractResponse()
                    {
                        ContractId = mc.ContractId,
                        ContractProducts = mc.ContractProducts,
                        IsDeleted = mc.IsDeleted,
                    });
                });
            }

            return result;
        }

        public static MerchantResponse ConvertFrom(Models.Merchant.Merchant merchant)
        {
            if (merchant == null) {
                return null;
            }

            MerchantResponse merchantResponse = new() {
                EstateId = merchant.EstateId,
                EstateReportingId = merchant.EstateReportingId,
                MerchantId = merchant.MerchantId,
                MerchantReportingId = merchant.MerchantReportingId,
                MerchantName = merchant.MerchantName,
                SettlementSchedule = ModelFactory.ConvertFrom(merchant.SettlementSchedule),
                MerchantReference = merchant.Reference,
                NextStatementDate = merchant.NextStatementDate
            };

            merchantResponse.Addresses = ConvertFrom(merchant.Addresses);
            merchantResponse.Contacts = ConvertFrom(merchant.Contacts);
            merchantResponse.Devices = ConvertFrom(merchant.Devices);
            merchantResponse.Operators = ConvertFrom(merchant.Operators);
            merchantResponse.Contracts = ConvertFrom(merchant.Contracts);

            return merchantResponse;
        }
    }
}