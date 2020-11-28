using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.Tests.Factories
{
    using DataTransferObjects;
    using Models;
    using Shouldly;
    using Testing;
    using TransactionProcessor.Factories;
    using Xunit;

    public class ModelFactoryTests
    {
        [Fact]
        public void ModelFactory_ProcessLogonTransactionResponseModel_IsConverted()
        {
            ProcessLogonTransactionResponse processLogonTransactionResponseModel = TestData.ProcessLogonTransactionResponseModel;

            ModelFactory modelFactory = new ModelFactory();

            SerialisedMessage logonTransactionResponse = modelFactory.ConvertFrom(processLogonTransactionResponseModel);

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

            ModelFactory modelFactory = new ModelFactory();

            SerialisedMessage logonTransactionResponse = modelFactory.ConvertFrom(processLogonTransactionResponseModel);

            logonTransactionResponse.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ProcessSaleTransactionResponseModel_IsConverted()
        {
            ProcessSaleTransactionResponse processSaleTransactionResponseModel = TestData.ProcessSaleTransactionResponseModel;

            ModelFactory modelFactory = new ModelFactory();

            SerialisedMessage saleTransactionResponse = modelFactory.ConvertFrom(processSaleTransactionResponseModel);

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

            ModelFactory modelFactory = new ModelFactory();

            SerialisedMessage saleTransactionResponse = modelFactory.ConvertFrom(processSaleTransactionResponseModel);

            saleTransactionResponse.ShouldBeNull();
        }

        [Fact]
        public void ModelFactory_ProcessReconciliationTransactionResponse_IsConverted()
        {
            ProcessReconciliationTransactionResponse processReconciliationTransactionResponseModel = TestData.ProcessReconciliationTransactionResponseModel;

            ModelFactory modelFactory = new ModelFactory();

            SerialisedMessage processReconciliationTransactionResponse = modelFactory.ConvertFrom(processReconciliationTransactionResponseModel);

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

            ModelFactory modelFactory = new ModelFactory();

            SerialisedMessage processReconciliationTransactionResponse = modelFactory.ConvertFrom(processReconciliationTransactionResponseModel);

            processReconciliationTransactionResponse.ShouldBeNull();
        }
    }
}
