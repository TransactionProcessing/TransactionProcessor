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

            LogonTransactionResponse logonTransactionResponse = modelFactory.ConvertFrom(processLogonTransactionResponseModel);

            logonTransactionResponse.ShouldNotBeNull();
            logonTransactionResponse.ResponseMessage.ShouldBe(processLogonTransactionResponseModel.ResponseMessage);
            logonTransactionResponse.ResponseCode.ShouldBe(processLogonTransactionResponseModel.ResponseCode);
        }

        [Fact]
        public void ModelFactory_ProcessLogonTransactionResponseModel_NullInput_IsConverted()
        {
            ProcessLogonTransactionResponse processLogonTransactionResponseModel = null;

            ModelFactory modelFactory = new ModelFactory();

            LogonTransactionResponse logonTransactionResponse = modelFactory.ConvertFrom(processLogonTransactionResponseModel);

            logonTransactionResponse.ShouldBeNull();
        }
    }
}
