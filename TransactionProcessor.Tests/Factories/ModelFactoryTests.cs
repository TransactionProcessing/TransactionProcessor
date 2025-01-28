using System;
using System.Collections.Generic;
using System.Text;
using TransactionProcessor.Aggregates;

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

            SerialisedMessage logonTransactionResponse = ModelFactory.ConvertFrom(processLogonTransactionResponseModel);

            logonTransactionResponse.ShouldBeNull();
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

            SerialisedMessage saleTransactionResponse = ModelFactory.ConvertFrom(processSaleTransactionResponseModel);

            saleTransactionResponse.ShouldBeNull();
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
            
            SerialisedMessage processReconciliationTransactionResponse = ModelFactory.ConvertFrom(processReconciliationTransactionResponseModel);

            processReconciliationTransactionResponse.ShouldBeNull();
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
            GetVoucherResponse dto = ModelFactory.ConvertFrom(model);

            dto.ShouldBeNull();
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
            DataTransferObjects.RedeemVoucherResponse dto = ModelFactory.ConvertFrom(model);

            dto.ShouldBeNull();
        }
    }
}
