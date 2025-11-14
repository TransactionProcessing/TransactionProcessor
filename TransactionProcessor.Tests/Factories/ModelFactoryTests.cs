using System;
using System.Collections.Generic;
using System.Linq;
using TransactionProcessor.Aggregates;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.DataTransferObjects.Responses.Operator;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Models.Estate;
using TransactionProcessor.Models.Merchant;
using Contract = TransactionProcessor.Models.Contract.Contract;
using ContractProductTransactionFee = TransactionProcessor.DataTransferObjects.Responses.Contract.ContractProductTransactionFee;
using ProductType = TransactionProcessor.DataTransferObjects.Responses.Contract.ProductType;

namespace TransactionProcessor.Tests.Factories
{
    using DataTransferObjects;
    using Models;
    using Shouldly;
    using Testing;
    using TransactionProcessor.Factories;
    using Xunit;
    using IssueVoucherResponse = Models.IssueVoucherResponse;
    using RedeemVoucherResponse = Models.RedeemVoucherResponse;

    public class ModelFactoryTests
    {
        [Fact]
        public void ModelFactory_Contract_ContractOnly_IsConverted()
        {
            Contract contractModel = TestData.ContractModel;

            ContractResponse contractResponse = ModelFactory.ConvertFrom(contractModel);

            contractResponse.ShouldNotBeNull();
            contractResponse.OperatorId.ShouldBe(contractModel.OperatorId);
            contractResponse.OperatorName.ShouldBe(contractModel.OperatorName);
            contractResponse.ContractId.ShouldBe(contractModel.ContractId);
            contractResponse.Description.ShouldBe(contractModel.Description);
            contractResponse.Products.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_Contract_ContractWithProducts_IsConverted()
        {
            Contract contractModel = TestData.ContractModelWithProducts;

            ContractResponse contractResponse = ModelFactory.ConvertFrom(contractModel);
            
            contractResponse.ShouldNotBeNull();
            contractResponse.OperatorId.ShouldBe(contractModel.OperatorId);
            contractResponse.ContractId.ShouldBe(contractModel.ContractId);
            contractResponse.Description.ShouldBe(contractModel.Description);
            contractResponse.Products.ShouldNotBeNull();
            contractResponse.Products.ShouldHaveSingleItem();

            ContractProduct contractProduct = contractResponse.Products.Single();
            Product expectedContractProduct = contractModel.Products.Single();

            contractProduct.ProductId.ShouldBe(expectedContractProduct.ContractProductId);
            contractProduct.Value.ShouldBe(expectedContractProduct.Value);
            contractProduct.DisplayText.ShouldBe(expectedContractProduct.DisplayText);
            contractProduct.Name.ShouldBe(expectedContractProduct.Name);
            contractProduct.ProductType.ShouldBe(Enum.Parse<ProductType>(expectedContractProduct.ProductType.ToString()));
            contractProduct.TransactionFees.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_EstateList_IsConverted()
        {
            List<Estate> estateModel = [TestData.EstateModel];

            List<EstateResponse> estateResponse = ModelFactory.ConvertFrom(estateModel);
            estateResponse.Count.ShouldBe(1);
            estateResponse.Single().EstateId.ShouldBe(TestData.EstateModel.EstateId);
            estateResponse.Single().EstateName.ShouldBe(TestData.EstateModel.Name);
        }

        [Fact]
        public void ModelFactory_EstateList_NullModelInList_IsConverted()
        {
            List<Estate> estateModel = [null];

            List<EstateResponse> estateResponse = ModelFactory.ConvertFrom(estateModel);
            estateResponse.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_MerchantList_IsConverted()
        {
            List<Merchant> merchantModelList = new List<Merchant>{
                TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts()
            };

            List<MerchantResponse> merchantResponseList = ModelFactory.ConvertFrom(merchantModelList);
            merchantResponseList.ShouldNotBeNull();
            merchantResponseList.ShouldNotBeEmpty();
            merchantResponseList.Count.ShouldBe(merchantModelList.Count);
        }

        [Fact]
        public void ModelFactory_MerchantList_NullList_IsConverted() {
            List<Merchant> merchantModelList = null;

            List<MerchantResponse> merchantResponseList = ModelFactory.ConvertFrom(merchantModelList);
            merchantResponseList.ShouldNotBeNull();
            merchantResponseList.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_MerchantList_NullModelInList_IsConverted()
        {
            List<Merchant> merchantModelList = new List<Merchant>{
                null
            };

            List<MerchantResponse> result = ModelFactory.ConvertFrom(merchantModelList);
            result.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_Contract_ContractWithProductsAndFees_IsConverted()
        {
            Contract contractModel = TestData.ContractModelWithProductsAndTransactionFees;

            ContractResponse contractResponse = ModelFactory.ConvertFrom(contractModel);

            contractResponse.ShouldNotBeNull();
            contractResponse.OperatorId.ShouldBe(contractModel.OperatorId);
            contractResponse.ContractId.ShouldBe(contractModel.ContractId);
            contractResponse.Description.ShouldBe(contractModel.Description);
            contractResponse.Products.ShouldNotBeNull();
            contractResponse.Products.ShouldHaveSingleItem();

            ContractProduct contractProduct = contractResponse.Products.Single();
            Product expectedContractProduct = contractModel.Products.Single();

            contractProduct.ProductId.ShouldBe(expectedContractProduct.ContractProductId);
            contractProduct.Value.ShouldBe(expectedContractProduct.Value);
            contractProduct.DisplayText.ShouldBe(expectedContractProduct.DisplayText);
            contractProduct.Name.ShouldBe(expectedContractProduct.Name);
            contractProduct.ProductType.ShouldBe(Enum.Parse<ProductType>(expectedContractProduct.ProductType.ToString()));
            contractProduct.TransactionFees.ShouldNotBeNull();
            contractProduct.TransactionFees.ShouldHaveSingleItem();

            DataTransferObjects.Responses.Contract.ContractProductTransactionFee productTransactionFee = contractProduct.TransactionFees.Single();
            Models.Contract.ContractProductTransactionFee expectedProductTransactionFee = expectedContractProduct.TransactionFees.Single();

            productTransactionFee.TransactionFeeId.ShouldBe(expectedProductTransactionFee.TransactionFeeId);
            productTransactionFee.Value.ShouldBe(expectedProductTransactionFee.Value);
            productTransactionFee.CalculationType.ShouldBe(Enum.Parse<DataTransferObjects.Responses.Contract.CalculationType>(expectedProductTransactionFee.CalculationType.ToString()));
            productTransactionFee.Description.ShouldBe(expectedProductTransactionFee.Description);
        }

        [Fact]
        public void ModelFactory_ContractList_IsConverted()
        {
            List<Contract> contractModel = new List<Contract>{
                TestData.ContractModel
            };

            List<ContractResponse> contractResponses = ModelFactory.ConvertFrom(contractModel);
            contractResponses.ShouldNotBeNull();
            contractResponses.ShouldHaveSingleItem();
            contractResponses.Single().OperatorId.ShouldBe(contractModel.Single().OperatorId);
            contractResponses.Single().ContractId.ShouldBe(contractModel.Single().ContractId);
            contractResponses.Single().Description.ShouldBe(contractModel.Single().Description);
            contractResponses.Single().Products.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ContractList_ModelInListIsNull_IsConverted()
        {
            List<Contract> contractModel = new List<Contract>{
                null
            };

            var contractResponses = ModelFactory.ConvertFrom(contractModel);
            contractResponses.Any().ShouldBeFalse();
        }

        [Fact]
        public void ModelFactory_ProcessLogonTransactionResponseModel_IsConverted()
        {
            ProcessLogonTransactionResponse processLogonTransactionResponseModel = TestData.ProcessLogonTransactionResponseModel;

            SerialisedMessage logonTransactionResponse = ModelFactory.ConvertFrom(processLogonTransactionResponseModel);
            logonTransactionResponse.ShouldNotBeNull();
            logonTransactionResponse.Metadata.ShouldContainKey(MetadataContants.KeyNameEstateId);
            logonTransactionResponse.Metadata.ShouldContainKey(MetadataContants.KeyNameMerchantId);
            String estateId = logonTransactionResponse.Metadata[MetadataContants.KeyNameEstateId];
            String merchantId = logonTransactionResponse.Metadata[MetadataContants.KeyNameMerchantId];
            estateId.ShouldBe(TestData.ProcessLogonTransactionResponseModel.EstateId.ToString());
            merchantId.ShouldBe(TestData.ProcessLogonTransactionResponseModel.MerchantId.ToString());
        }

        [Fact]
        public void ModelFactory_ProcessLogonTransactionResponseModel_NullInput_IsConverted()
        {
            ProcessLogonTransactionResponse processLogonTransactionResponseModel = null;

            SerialisedMessage logonTransactionResult = ModelFactory.ConvertFrom(processLogonTransactionResponseModel);
            logonTransactionResult.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ProcessSaleTransactionResponseModel_IsConverted()
        {
            ProcessSaleTransactionResponse processSaleTransactionResponseModel = TestData.ProcessSaleTransactionResponseModel;

            SerialisedMessage saleTransactionResponse = ModelFactory.ConvertFrom(processSaleTransactionResponseModel);

            saleTransactionResponse.ShouldNotBeNull();
            saleTransactionResponse.Metadata.ShouldContainKey(MetadataContants.KeyNameEstateId);
            saleTransactionResponse.Metadata.ShouldContainKey(MetadataContants.KeyNameMerchantId);
            String estateId = saleTransactionResponse.Metadata[MetadataContants.KeyNameEstateId];
            String merchantId = saleTransactionResponse.Metadata[MetadataContants.KeyNameMerchantId];
            estateId.ShouldBe(TestData.ProcessSaleTransactionResponseModel.EstateId.ToString());
            merchantId.ShouldBe(TestData.ProcessSaleTransactionResponseModel.MerchantId.ToString());
        }

        [Fact]
        public void ModelFactory_ProcessSaleTransactionResponseModel_NullInput_IsConverted()
        {
            ProcessSaleTransactionResponse processSaleTransactionResponseModel = null;

            SerialisedMessage saleTransactionResult = ModelFactory.ConvertFrom(processSaleTransactionResponseModel);
            saleTransactionResult.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ProcessReconciliationTransactionResponse_IsConverted()
        {
            ProcessReconciliationTransactionResponse processReconciliationTransactionResponseModel = TestData.ProcessReconciliationTransactionResponseModel;

            SerialisedMessage processReconciliationTransactionResponse = ModelFactory.ConvertFrom(processReconciliationTransactionResponseModel);

            processReconciliationTransactionResponse.ShouldNotBeNull();
            processReconciliationTransactionResponse.Metadata.ShouldContainKey(MetadataContants.KeyNameEstateId);
            processReconciliationTransactionResponse.Metadata.ShouldContainKey(MetadataContants.KeyNameMerchantId);
            String estateId = processReconciliationTransactionResponse.Metadata[MetadataContants.KeyNameEstateId];
            String merchantId = processReconciliationTransactionResponse.Metadata[MetadataContants.KeyNameMerchantId];
            estateId.ShouldBe(TestData.ProcessSaleTransactionResponseModel.EstateId.ToString());
            merchantId.ShouldBe(TestData.ProcessSaleTransactionResponseModel.MerchantId.ToString());
        }

        [Fact]
        public void ModelFactory_ProcessReconciliationTransactionResponse_NullInput_IsConverted()
        {
            ProcessReconciliationTransactionResponse processReconciliationTransactionResponseModel = null;

            SerialisedMessage reconciliationTransactionResult = ModelFactory.ConvertFrom(processReconciliationTransactionResponseModel);
            reconciliationTransactionResult.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IssueVoucherResponse_IsConverted()
        {
            IssueVoucherResponse model = TestData.IssueVoucherResponse;
            DataTransferObjects.IssueVoucherResponse dto = ModelFactory.ConvertFrom(model);

            dto.ShouldNotBeNull();
            dto.ExpiryDate.ShouldBe(model.ExpiryDate);
            dto.Message.ShouldBe(model.Message);
            dto.VoucherCode.ShouldBe(model.VoucherCode);
            dto.VoucherId.ShouldBe(model.VoucherId);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_IssueVoucherResponse_NullInput_IsConverted()
        {
            IssueVoucherResponse model = null;
            DataTransferObjects.IssueVoucherResponse dto = ModelFactory.ConvertFrom(model);

            dto.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_VoucherModel_IsConverted()
        {
            Voucher model = TestData.GetVoucherAggregateWithRecipientMobile().GetVoucher();

            GetVoucherResponse dto = ModelFactory.ConvertFrom(model);
            dto.ShouldNotBeNull();
            dto.TransactionId.ShouldBe(model.TransactionId);
            dto.IssuedDateTime.ShouldBe(model.IssuedDateTime);
            dto.Balance.ShouldBe(model.Balance);
            dto.IssuedDateTime.ShouldBe(model.IssuedDateTime);
            dto.ExpiryDate.ShouldBe(model.ExpiryDate);
            dto.GeneratedDateTime.ShouldBe(model.GeneratedDateTime);
            dto.IsGenerated.ShouldBe(model.IsGenerated);
            dto.IsIssued.ShouldBe(model.IsIssued);
            dto.IsRedeemed.ShouldBe(model.IsRedeemed);
            dto.RedeemedDateTime.ShouldBe(model.RedeemedDateTime);
            dto.Value.ShouldBe(model.Value);
            dto.VoucherCode.ShouldBe(model.VoucherCode);
            dto.Balance.ShouldBe(model.Balance);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_VoucherModel_NullInput_IsConverted()
        {
            Voucher model = null;
            GetVoucherResponse result = ModelFactory.ConvertFrom(model);

            result.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_RedeemVoucherResponse_IsConverted()
        {
            RedeemVoucherResponse model = TestData.RedeemVoucherResponse;
            
            DataTransferObjects.RedeemVoucherResponse dto = ModelFactory.ConvertFrom(model);
            dto.ShouldNotBeNull();
            dto.ExpiryDate.ShouldBe(model.ExpiryDate);
            dto.VoucherCode.ShouldBe(model.VoucherCode);
            dto.RemainingBalance.ShouldBe(model.RemainingBalance);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_RedeemVoucherResponse_NullInput_IsConverted()
        {
            RedeemVoucherResponse model = null;
            DataTransferObjects.RedeemVoucherResponse result = ModelFactory.ConvertFrom(model);
            result.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_Operator_ModelConverted()
        {
            OperatorResponse operatorResponse = ModelFactory.ConvertFrom(TestData.OperatorModel);
            operatorResponse.ShouldNotBeNull();
            operatorResponse.OperatorId.ShouldBe(TestData.OperatorId);
            operatorResponse.RequireCustomTerminalNumber.ShouldBe(TestData.RequireCustomTerminalNumber);
            operatorResponse.RequireCustomMerchantNumber.ShouldBe(TestData.RequireCustomMerchantNumber);
            operatorResponse.Name.ShouldBe(TestData.OperatorName);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_OperatorModelIsNull_ModelConverted()
        {
            TransactionProcessor.Models.Operator.Operator operatorModel = null;

            OperatorResponse operatorResponseResult = ModelFactory.ConvertFrom(operatorModel);
            operatorResponseResult.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_OperatorList_ModelConverted()
        {
            List<TransactionProcessor.Models.Operator.Operator> operatorList = new List<TransactionProcessor.Models.Operator.Operator>{
                                                                TestData.OperatorModel
                                                            };

            List<OperatorResponse> operatorResponses = ModelFactory.ConvertFrom(operatorList);
            operatorResponses.ShouldNotBeNull();
            operatorResponses.ShouldNotBeEmpty();
            operatorResponses.Count.ShouldBe(operatorList.Count);
            foreach (OperatorResponse operatorResponse in operatorResponses)
            {
                TransactionProcessor.Models.Operator.Operator @operator = operatorList.SingleOrDefault(o => o.OperatorId == operatorResponse.OperatorId);
                @operator.ShouldNotBeNull();
                @operator.OperatorId.ShouldBe(operatorResponse.OperatorId);
                @operator.RequireCustomTerminalNumber.ShouldBe(operatorResponse.RequireCustomTerminalNumber);
                @operator.RequireCustomMerchantNumber.ShouldBe(operatorResponse.RequireCustomMerchantNumber);
                @operator.Name.ShouldBe(operatorResponse.Name);
            }
        }

        [Fact]
        public void ModelFactory_ConvertFrom_NullOperatorList_ModelConverted()
        {
            List<TransactionProcessor.Models.Operator.Operator> operatorList = null;

            List<OperatorResponse> operatorResponse = ModelFactory.ConvertFrom(operatorList);
            operatorResponse.ShouldBeEmpty();

        }

        [Fact]
        public void ModelFactory_ConvertFrom_EmptyOperatorList_ModelConverted()
        {
            List<TransactionProcessor.Models.Operator.Operator> operatorList = new List<TransactionProcessor.Models.Operator.Operator>();

            List<OperatorResponse> operatorResponse = ModelFactory.ConvertFrom(operatorList);
            operatorResponse.ShouldBeEmpty();

        }

        [Fact]
        public void ModelFactory_ConvertFrom_NullModelInList_ModelConverted()
        {
            List<TransactionProcessor.Models.Operator.Operator> operatorList = new List<TransactionProcessor.Models.Operator.Operator> {
                null
            };

            List<OperatorResponse> result = ModelFactory.ConvertFrom(operatorList);
            result.ShouldBeEmpty();

        }

        [Fact]
        public void ModelFactory_Estate_NullInput_IsConverted()
        {
            Estate estateModel = null;

            EstateResponse estateResponseResult = ModelFactory.ConvertFrom(estateModel);

            estateResponseResult.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_Estate_WithNoOperatorsOrSecurityUsers_IsConverted()
        {
            Estate estateModel = TestData.EstateModel;

            EstateResponse estateResponse= ModelFactory.ConvertFrom(estateModel);
            estateResponse.ShouldNotBeNull();
            estateResponse.EstateId.ShouldBe(estateModel.EstateId);
            estateResponse.EstateName.ShouldBe(estateModel.Name);
        }

        [Fact]
        public void ModelFactory_Estate_WithOperators_IsConverted()
        {
            Estate estateModel = TestData.EstateModelWithOperators;

            
            EstateResponse estateResponse = ModelFactory.ConvertFrom(estateModel);
            estateResponse.ShouldNotBeNull();
            estateResponse.EstateId.ShouldBe(estateModel.EstateId);
            estateResponse.EstateName.ShouldBe(estateModel.Name);
            estateResponse.Operators.ShouldNotBeNull();
            estateResponse.Operators.Count.ShouldBe(estateModel.Operators.Count);
            estateResponse.SecurityUsers.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_Estate_WithSecurityUsers_IsConverted()
        {
            Estate estateModel = TestData.EstateModelWithSecurityUsers;
            
            EstateResponse estateResponse = ModelFactory.ConvertFrom(estateModel);
            estateResponse.ShouldNotBeNull();
            estateResponse.EstateId.ShouldBe(estateModel.EstateId);
            estateResponse.EstateName.ShouldBe(estateModel.Name);
            estateResponse.Operators.ShouldBeEmpty();
            estateResponse.SecurityUsers.ShouldNotBeNull();
            estateResponse.SecurityUsers.Count.ShouldBe(estateModel.SecurityUsers.Count);
        }

        [Fact]
        public void ModelFactory_Estate_WithOperatorsAndSecurityUsers_IsConverted()
        {
            Estate estateModel = TestData.EstateModelWithOperatorsAndSecurityUsers;

            EstateResponse estateResponse = ModelFactory.ConvertFrom(estateModel);
            estateResponse.ShouldNotBeNull();
            estateResponse.EstateId.ShouldBe(estateModel.EstateId);
            estateResponse.EstateName.ShouldBe(estateModel.Name);
            estateResponse.Operators.ShouldNotBeNull();
            estateResponse.Operators.Count.ShouldBe(estateModel.Operators.Count);
            estateResponse.SecurityUsers.ShouldNotBeNull();
            estateResponse.SecurityUsers.Count.ShouldBe(estateModel.SecurityUsers.Count);
        }

        [Theory]
        [InlineData(Models.Merchant.SettlementSchedule.NotSet)]
        [InlineData(Models.Merchant.SettlementSchedule.Immediate)]
        [InlineData(Models.Merchant.SettlementSchedule.Weekly)]
        [InlineData(Models.Merchant.SettlementSchedule.Monthly)]
        public void ModelFactory_Merchant_IsConverted(Models.Merchant.SettlementSchedule settlementSchedule)
        {
            Merchant merchantModel = TestData.MerchantModelWithAddressesContactsDevicesAndOperatorsAndContracts(settlementSchedule);

            MerchantResponse merchantResponse = ModelFactory.ConvertFrom(merchantModel);
            merchantResponse.ShouldNotBeNull();
            merchantResponse.MerchantId.ShouldBe(merchantModel.MerchantId);
            merchantResponse.MerchantName.ShouldBe(merchantModel.MerchantName);
            merchantResponse.Addresses.ShouldHaveSingleItem();

            AddressResponse addressResponse = merchantResponse.Addresses.Single();
            addressResponse.AddressId.ShouldBe(merchantModel.Addresses.Single().AddressId);
            addressResponse.AddressLine1.ShouldBe(merchantModel.Addresses.Single().AddressLine1);
            addressResponse.AddressLine2.ShouldBe(merchantModel.Addresses.Single().AddressLine2);
            addressResponse.AddressLine3.ShouldBe(merchantModel.Addresses.Single().AddressLine3);
            addressResponse.AddressLine4.ShouldBe(merchantModel.Addresses.Single().AddressLine4);
            addressResponse.Town.ShouldBe(merchantModel.Addresses.Single().Town);
            addressResponse.Region.ShouldBe(merchantModel.Addresses.Single().Region);
            addressResponse.Country.ShouldBe(merchantModel.Addresses.Single().Country);
            addressResponse.PostalCode.ShouldBe(merchantModel.Addresses.Single().PostalCode);

            merchantResponse.Contacts.ShouldHaveSingleItem();
            ContactResponse contactResponse = merchantResponse.Contacts.Single();
            contactResponse.ContactId.ShouldBe(merchantModel.Contacts.Single().ContactId);
            contactResponse.ContactEmailAddress.ShouldBe(merchantModel.Contacts.Single().ContactEmailAddress);
            contactResponse.ContactName.ShouldBe(merchantModel.Contacts.Single().ContactName);
            contactResponse.ContactPhoneNumber.ShouldBe(merchantModel.Contacts.Single().ContactPhoneNumber);
        }

        [Fact]
        public void ModelFactory_Merchant_NullInput_IsConverted()
        {
            Merchant merchantModel = null;

            MerchantResponse result = ModelFactory.ConvertFrom(merchantModel);
            result.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_Merchant_NullAddresses_IsConverted()
        {
            Merchant merchantModel = TestData.MerchantModelWithNullAddresses;

            MerchantResponse merchantResponse = ModelFactory.ConvertFrom(merchantModel);

            merchantResponse.ShouldNotBeNull();
            merchantResponse.MerchantId.ShouldBe(merchantModel.MerchantId);
            merchantResponse.MerchantName.ShouldBe(merchantModel.MerchantName);

            merchantResponse.Addresses.ShouldBeEmpty();

            merchantResponse.Contacts.ShouldHaveSingleItem();
            ContactResponse contactResponse = merchantResponse.Contacts.Single();
            contactResponse.ContactId.ShouldBe(merchantModel.Contacts.Single().ContactId);
            contactResponse.ContactEmailAddress.ShouldBe(merchantModel.Contacts.Single().ContactEmailAddress);
            contactResponse.ContactName.ShouldBe(merchantModel.Contacts.Single().ContactName);
            contactResponse.ContactPhoneNumber.ShouldBe(merchantModel.Contacts.Single().ContactPhoneNumber);

            merchantResponse.Devices.ShouldHaveSingleItem();
            KeyValuePair<Guid, String> device = merchantResponse.Devices.Single();
            device.Key.ShouldBe(merchantModel.Devices.Single().DeviceId);
            device.Value.ShouldBe(merchantModel.Devices.Single().DeviceIdentifier);

            merchantResponse.Operators.ShouldHaveSingleItem();
            MerchantOperatorResponse operatorDetails = merchantResponse.Operators.Single();
            operatorDetails.Name.ShouldBe(merchantModel.Operators.Single().Name);
            operatorDetails.OperatorId.ShouldBe(merchantModel.Operators.Single().OperatorId);
            operatorDetails.MerchantNumber.ShouldBe(merchantModel.Operators.Single().MerchantNumber);
            operatorDetails.TerminalNumber.ShouldBe(merchantModel.Operators.Single().TerminalNumber);
        }

        [Fact]
        public void ModelFactory_Merchant_NullContacts_IsConverted()
        {
            Merchant merchantModel = TestData.MerchantModelWithNullContacts;

            MerchantResponse merchantResponse = ModelFactory.ConvertFrom(merchantModel);

            merchantResponse.ShouldNotBeNull();
            merchantResponse.MerchantId.ShouldBe(merchantModel.MerchantId);
            merchantResponse.MerchantName.ShouldBe(merchantModel.MerchantName);
            merchantResponse.Addresses.ShouldHaveSingleItem();

            AddressResponse addressResponse = merchantResponse.Addresses.Single();
            addressResponse.AddressId.ShouldBe(merchantModel.Addresses.Single().AddressId);
            addressResponse.AddressLine1.ShouldBe(merchantModel.Addresses.Single().AddressLine1);
            addressResponse.AddressLine2.ShouldBe(merchantModel.Addresses.Single().AddressLine2);
            addressResponse.AddressLine3.ShouldBe(merchantModel.Addresses.Single().AddressLine3);
            addressResponse.AddressLine4.ShouldBe(merchantModel.Addresses.Single().AddressLine4);
            addressResponse.Town.ShouldBe(merchantModel.Addresses.Single().Town);
            addressResponse.Region.ShouldBe(merchantModel.Addresses.Single().Region);
            addressResponse.Country.ShouldBe(merchantModel.Addresses.Single().Country);
            addressResponse.PostalCode.ShouldBe(merchantModel.Addresses.Single().PostalCode);

            merchantResponse.Contacts.ShouldBeEmpty();

            merchantResponse.Devices.ShouldHaveSingleItem();
            KeyValuePair<Guid, String> device = merchantResponse.Devices.Single();
            device.Key.ShouldBe(merchantModel.Devices.Single().DeviceId);
            device.Value.ShouldBe(merchantModel.Devices.Single().DeviceIdentifier);

            merchantResponse.Operators.ShouldHaveSingleItem();
            MerchantOperatorResponse operatorDetails = merchantResponse.Operators.Single();
            operatorDetails.Name.ShouldBe(merchantModel.Operators.Single().Name);
            operatorDetails.OperatorId.ShouldBe(merchantModel.Operators.Single().OperatorId);
            operatorDetails.MerchantNumber.ShouldBe(merchantModel.Operators.Single().MerchantNumber);
            operatorDetails.TerminalNumber.ShouldBe(merchantModel.Operators.Single().TerminalNumber);
        }

        [Fact]
        public void ModelFactory_Merchant_NullDevices_IsConverted()
        {
            Merchant merchantModel = TestData.MerchantModelWithNullDevices;

            MerchantResponse merchantResponse = ModelFactory.ConvertFrom(merchantModel);

            merchantResponse.ShouldNotBeNull();
            merchantResponse.MerchantId.ShouldBe(merchantModel.MerchantId);
            merchantResponse.MerchantName.ShouldBe(merchantModel.MerchantName);
            merchantResponse.Addresses.ShouldHaveSingleItem();

            AddressResponse addressResponse = merchantResponse.Addresses.Single();
            addressResponse.AddressId.ShouldBe(merchantModel.Addresses.Single().AddressId);
            addressResponse.AddressLine1.ShouldBe(merchantModel.Addresses.Single().AddressLine1);
            addressResponse.AddressLine2.ShouldBe(merchantModel.Addresses.Single().AddressLine2);
            addressResponse.AddressLine3.ShouldBe(merchantModel.Addresses.Single().AddressLine3);
            addressResponse.AddressLine4.ShouldBe(merchantModel.Addresses.Single().AddressLine4);
            addressResponse.Town.ShouldBe(merchantModel.Addresses.Single().Town);
            addressResponse.Region.ShouldBe(merchantModel.Addresses.Single().Region);
            addressResponse.Country.ShouldBe(merchantModel.Addresses.Single().Country);
            addressResponse.PostalCode.ShouldBe(merchantModel.Addresses.Single().PostalCode);

            merchantResponse.Contacts.ShouldHaveSingleItem();
            ContactResponse contactResponse = merchantResponse.Contacts.Single();
            contactResponse.ContactId.ShouldBe(merchantModel.Contacts.Single().ContactId);
            contactResponse.ContactEmailAddress.ShouldBe(merchantModel.Contacts.Single().ContactEmailAddress);
            contactResponse.ContactName.ShouldBe(merchantModel.Contacts.Single().ContactName);
            contactResponse.ContactPhoneNumber.ShouldBe(merchantModel.Contacts.Single().ContactPhoneNumber);

            merchantResponse.Devices.ShouldBeEmpty();

            merchantResponse.Operators.ShouldHaveSingleItem();
            MerchantOperatorResponse operatorDetails = merchantResponse.Operators.Single();
            operatorDetails.Name.ShouldBe(merchantModel.Operators.Single().Name);
            operatorDetails.OperatorId.ShouldBe(merchantModel.Operators.Single().OperatorId);
            operatorDetails.MerchantNumber.ShouldBe(merchantModel.Operators.Single().MerchantNumber);
            operatorDetails.TerminalNumber.ShouldBe(merchantModel.Operators.Single().TerminalNumber);
        }

        [Fact]
        public void ModelFactory_Merchant_NullOperators_IsConverted()
        {
            Merchant merchantModel = TestData.MerchantModelWithNullOperators;

            MerchantResponse merchantResponse = ModelFactory.ConvertFrom(merchantModel);

            merchantResponse.ShouldNotBeNull();
            merchantResponse.MerchantId.ShouldBe(merchantModel.MerchantId);
            merchantResponse.MerchantName.ShouldBe(merchantModel.MerchantName);
            merchantResponse.Addresses.ShouldHaveSingleItem();

            AddressResponse addressResponse = merchantResponse.Addresses.Single();
            addressResponse.AddressId.ShouldBe(merchantModel.Addresses.Single().AddressId);
            addressResponse.AddressLine1.ShouldBe(merchantModel.Addresses.Single().AddressLine1);
            addressResponse.AddressLine2.ShouldBe(merchantModel.Addresses.Single().AddressLine2);
            addressResponse.AddressLine3.ShouldBe(merchantModel.Addresses.Single().AddressLine3);
            addressResponse.AddressLine4.ShouldBe(merchantModel.Addresses.Single().AddressLine4);
            addressResponse.Town.ShouldBe(merchantModel.Addresses.Single().Town);
            addressResponse.Region.ShouldBe(merchantModel.Addresses.Single().Region);
            addressResponse.Country.ShouldBe(merchantModel.Addresses.Single().Country);
            addressResponse.PostalCode.ShouldBe(merchantModel.Addresses.Single().PostalCode);

            merchantResponse.Contacts.ShouldHaveSingleItem();
            ContactResponse contactResponse = merchantResponse.Contacts.Single();
            contactResponse.ContactId.ShouldBe(merchantModel.Contacts.Single().ContactId);
            contactResponse.ContactEmailAddress.ShouldBe(merchantModel.Contacts.Single().ContactEmailAddress);
            contactResponse.ContactName.ShouldBe(merchantModel.Contacts.Single().ContactName);
            contactResponse.ContactPhoneNumber.ShouldBe(merchantModel.Contacts.Single().ContactPhoneNumber);

            merchantResponse.Devices.ShouldHaveSingleItem();
            KeyValuePair<Guid, String> device = merchantResponse.Devices.Single();
            device.Key.ShouldBe(merchantModel.Devices.Single().DeviceId);
            device.Value.ShouldBe(merchantModel.Devices.Single().DeviceIdentifier);

            merchantResponse.Operators.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_TransactionFeeList_IsConverted()
        {
            List<Models.Contract.ContractProductTransactionFee> transactionFeeModelList = TestData.ProductTransactionFees;

            List<ContractProductTransactionFee> transactionFeeResponseList = ModelFactory.ConvertFrom(transactionFeeModelList);
            transactionFeeResponseList.ShouldNotBeNull();
            transactionFeeResponseList.ShouldNotBeEmpty();
            transactionFeeResponseList.Count.ShouldBe(transactionFeeModelList.Count);
        }
    }
}
