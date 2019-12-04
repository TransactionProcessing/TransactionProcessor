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
    }
}
