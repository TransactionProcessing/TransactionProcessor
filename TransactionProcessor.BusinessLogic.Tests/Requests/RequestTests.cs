﻿using System;
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
                                                                                                               TestData.AdditionalTransactionMetaData(),
                                                                                                               TestData.ContractId,
                                                                                                               TestData.ProductId,
                                                                                                               TestData.TransactionSource);

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
            processSaleTransactionRequest.AdditionalTransactionMetadata.Count.ShouldBe(TestData.AdditionalTransactionMetaData().Count);
            processSaleTransactionRequest.ContractId.ShouldBe(TestData.ContractId);
            processSaleTransactionRequest.ProductId.ShouldBe(TestData.ProductId);
            processSaleTransactionRequest.TransactionSource.ShouldBe(TestData.TransactionSource);
        }

        [Fact]
        public void ProcessReconciliationRequest_CanBeCreated_IsCreated()
        {
            ProcessReconciliationRequest processReconciliationRequest = ProcessReconciliationRequest.Create(TestData.TransactionId,
                                                                                                            TestData.EstateId,
                                                                                                            TestData.MerchantId,
                                                                                                            TestData.DeviceIdentifier,
                                                                                                            TestData.TransactionDateTime,
                                                                                                            TestData.ReconciliationTransactionCount,
                                                                                                            TestData.ReconciliationTransactionValue);

            processReconciliationRequest.ShouldNotBeNull();
            processReconciliationRequest.EstateId.ShouldBe(TestData.EstateId);
            processReconciliationRequest.MerchantId.ShouldBe(TestData.MerchantId);
            processReconciliationRequest.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
            processReconciliationRequest.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            processReconciliationRequest.TransactionCount.ShouldBe(TestData.ReconciliationTransactionCount);
            processReconciliationRequest.TransactionValue.ShouldBe(TestData.ReconciliationTransactionValue);
            processReconciliationRequest.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public void ProcessSettlementRequest_CanBeCreated_IsCreated()
        {
            ProcessSettlementRequest processSettlementRequest = ProcessSettlementRequest.Create(TestData.SettlementDate, 
                                                                                                TestData.EstateId);

            processSettlementRequest.ShouldNotBeNull();
            processSettlementRequest.EstateId.ShouldBe(TestData.EstateId);
            processSettlementRequest.SettlementDate.ShouldBe(TestData.SettlementDate);

        }
    }
}
