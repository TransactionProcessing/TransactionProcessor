using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.Tests.Commands
{
    using Requests;
    using Shouldly;
    using Testing;
    using Xunit;

    public class RequestTests
    {
        [Fact]
        public void ProcessLogonTransactionRequest_CanBeCreated_IsCreated()
        {
            ProcessLogonTransactionRequest processLogonTransactionRequest = ProcessLogonTransactionRequest.Create(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,TestData.TransactionTypeLogon.ToString(), TestData.TransactionDateTime,
                                                                                                                  TestData.TransactionNumber);

            processLogonTransactionRequest.ShouldNotBeNull();
            processLogonTransactionRequest.EstateId.ShouldBe(TestData.EstateId);
            processLogonTransactionRequest.MerchantId.ShouldBe(TestData.MerchantId);
            processLogonTransactionRequest.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
            processLogonTransactionRequest.TransactionType.ShouldBe(TestData.TransactionTypeLogon.ToString());
            processLogonTransactionRequest.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            processLogonTransactionRequest.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            processLogonTransactionRequest.TransactionId.ShouldBe(processLogonTransactionRequest.TransactionId);
        }

        [Fact]
        public void ProcessSaleTransactionRequest_CanBeCreated_IsCreated()
        {
            ProcessSaleTransactionRequest processSaleTransactionRequest = ProcessSaleTransactionRequest.Create(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier, TestData.TransactionTypeLogon.ToString(), TestData.TransactionDateTime,
                                                                                                               TestData.TransactionNumber,
                                                                                                               TestData.OperatorIdentifier1,
                                                                                                               TestData.CustomerEmailAddress,
                                                                                                               TestData.AdditionalTransactionMetaData);

            processSaleTransactionRequest.ShouldNotBeNull();
            processSaleTransactionRequest.EstateId.ShouldBe(TestData.EstateId);
            processSaleTransactionRequest.MerchantId.ShouldBe(TestData.MerchantId);
            processSaleTransactionRequest.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
            processSaleTransactionRequest.TransactionType.ShouldBe(TestData.TransactionTypeLogon.ToString());
            processSaleTransactionRequest.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            processSaleTransactionRequest.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            processSaleTransactionRequest.TransactionId.ShouldBe(TestData.TransactionId);
            processSaleTransactionRequest.OperatorIdentifier.ShouldBe(TestData.OperatorIdentifier1);
            processSaleTransactionRequest.CustomerEmailAddress.ShouldBe(TestData.CustomerEmailAddress);
            processSaleTransactionRequest.AdditionalTransactionMetadata.ShouldNotBeNull();
            processSaleTransactionRequest.AdditionalTransactionMetadata.Count.ShouldBe(TestData.AdditionalTransactionMetaData.Count);
        }
    }
}
