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
            ProcessLogonTransactionRequest processLogonTransactionRequest = ProcessLogonTransactionRequest.Create(TestData.TransactionId, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier,TestData.TransactionType, TestData.TransactionDateTime,
                                                                                                                  TestData.TransactionNumber);

            processLogonTransactionRequest.ShouldNotBeNull();
            processLogonTransactionRequest.EstateId.ShouldBe(TestData.EstateId);
            processLogonTransactionRequest.MerchantId.ShouldBe(TestData.MerchantId);
            processLogonTransactionRequest.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
            processLogonTransactionRequest.TransactionType.ShouldBe(TestData.TransactionType);
            processLogonTransactionRequest.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            processLogonTransactionRequest.TransactionNumber.ShouldBe(TestData.TransactionNumber);
        }
    }
}
