﻿namespace TransactionProcessor.BusinessLogic.Tests.Commands
{
    using Requests;
    using Shouldly;
    using Testing;
    using Xunit;

    public class RequestTests
    {
        #region Methods

        [Fact]
        public void ProcessLogonTransactionRequest_CanBeCreated_IsCreated() {
            ProcessLogonTransactionRequest processLogonTransactionRequest =
                ProcessLogonTransactionRequest.Create(TestData.TransactionId,
                                                      TestData.EstateId,
                                                      TestData.MerchantId,
                                                      TestData.DeviceIdentifier,
                                                      TestData.TransactionTypeLogon.ToString(),
                                                      TestData.TransactionDateTime,
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
        public void ProcessReconciliationRequest_CanBeCreated_IsCreated() {
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
        public void ProcessSaleTransactionRequest_CanBeCreated_IsCreated() {
            ProcessSaleTransactionRequest processSaleTransactionRequest = ProcessSaleTransactionRequest.Create(TestData.TransactionId,
                                                                                                               TestData.EstateId,
                                                                                                               TestData.MerchantId,
                                                                                                               TestData.DeviceIdentifier,
                                                                                                               TestData.TransactionTypeLogon.ToString(),
                                                                                                               TestData.TransactionDateTime,
                                                                                                               TestData.TransactionNumber,
                                                                                                               TestData.OperatorId,
                                                                                                               TestData.CustomerEmailAddress,
                                                                                                               TestData.AdditionalTransactionMetaDataForMobileTopup(),
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
            processSaleTransactionRequest.OperatorId.ShouldBe(TestData.OperatorId);
            processSaleTransactionRequest.CustomerEmailAddress.ShouldBe(TestData.CustomerEmailAddress);
            processSaleTransactionRequest.AdditionalTransactionMetadata.ShouldNotBeNull();
            processSaleTransactionRequest.AdditionalTransactionMetadata.Count.ShouldBe(TestData.AdditionalTransactionMetaDataForMobileTopup().Count);
            processSaleTransactionRequest.ContractId.ShouldBe(TestData.ContractId);
            processSaleTransactionRequest.ProductId.ShouldBe(TestData.ProductId);
            processSaleTransactionRequest.TransactionSource.ShouldBe(TestData.TransactionSource);
        }

        [Fact]
        public void ProcessSettlementRequest_CanBeCreated_IsCreated() {
            ProcessSettlementRequest processSettlementRequest = ProcessSettlementRequest.Create(TestData.SettlementDate, 
                                                                                                TestData.MerchantId,
                                                                                                TestData.EstateId);

            processSettlementRequest.ShouldNotBeNull();
            processSettlementRequest.EstateId.ShouldBe(TestData.EstateId);
            processSettlementRequest.MerchantId.ShouldBe(TestData.MerchantId);
            processSettlementRequest.SettlementDate.ShouldBe(TestData.SettlementDate);
        }

        [Fact]
        public void Test_CanBeCreated_IsCreated() {
            ResendTransactionReceiptRequest resendTransactionReceiptRequest = ResendTransactionReceiptRequest.Create(TestData.TransactionId,
                 TestData.EstateId);

            resendTransactionReceiptRequest.ShouldNotBeNull();
            resendTransactionReceiptRequest.EstateId.ShouldBe(TestData.EstateId);
            resendTransactionReceiptRequest.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public void IssueVoucherRequest_CanBeCreated_IsCreated()
        {
            IssueVoucherRequest issueVoucherRequest = IssueVoucherRequest.Create(TestData.VoucherId,
                                                                                 TestData.OperatorId,
                                                                                 TestData.EstateId,
                                                                                 TestData.TransactionId,
                                                                                 TestData.IssuedDateTime,
                                                                                 TestData.Value,
                                                                                 TestData.RecipientEmail,
                                                                                 TestData.RecipientMobile);

            issueVoucherRequest.ShouldNotBeNull();
            issueVoucherRequest.VoucherId.ShouldBe(TestData.VoucherId);
            issueVoucherRequest.OperatorId.ShouldBe(TestData.OperatorId);
            issueVoucherRequest.EstateId.ShouldBe(TestData.EstateId);
            issueVoucherRequest.TransactionId.ShouldBe(TestData.TransactionId);
            issueVoucherRequest.Value.ShouldBe(TestData.Value);
            issueVoucherRequest.RecipientEmail.ShouldBe(TestData.RecipientEmail);
            issueVoucherRequest.RecipientMobile.ShouldBe(TestData.RecipientMobile);
            issueVoucherRequest.IssuedDateTime.ShouldBe(TestData.IssuedDateTime);
        }

        [Fact]
        public void RedeemVoucherRequest_CanBeCreated_IsCreated()
        {
            RedeemVoucherRequest redeemVoucherRequest = RedeemVoucherRequest.Create(TestData.EstateId, TestData.VoucherCode, TestData.RedeemedDateTime);

            redeemVoucherRequest.ShouldNotBeNull();
            redeemVoucherRequest.VoucherCode.ShouldBe(TestData.VoucherCode);
            redeemVoucherRequest.RedeemedDateTime.ShouldBe(TestData.RedeemedDateTime);
            redeemVoucherRequest.EstateId.ShouldBe(TestData.EstateId);
        }

        [Fact]
        public void CreateFloatForContractProductRequest_CanBeCreated_IsCreated(){
            CreateFloatForContractProductRequest createFloatForContractProductRequest = CreateFloatForContractProductRequest.Create(TestData.EstateId, TestData.ContractId, TestData.ProductId,
                                                                                                      TestData.FloatCreatedDateTime);

            createFloatForContractProductRequest.ShouldNotBeNull();
            createFloatForContractProductRequest.ContractId.ShouldBe(TestData.ContractId);
            createFloatForContractProductRequest.ProductId.ShouldBe(TestData.ProductId);
            createFloatForContractProductRequest.CreateDateTime.ShouldBe(TestData.FloatCreatedDateTime);
            createFloatForContractProductRequest.EstateId.ShouldBe(TestData.EstateId);
        }

        [Fact]
        public void RecordCreditPurchaseForFloatRequest_CanBeCreated_IsCreated(){
            RecordCreditPurchaseForFloatRequest recordCreditPurchaseForFloatRequest = RecordCreditPurchaseForFloatRequest.Create(TestData.EstateId,
                                                                                                                                 TestData.FloatAggregateId,
                                                                                                                                 TestData.FloatCreditAmount,
                                                                                                                                 TestData.FloatCreditCostPrice,
                                                                                                                                 TestData.CreditPurchasedDateTime);
            recordCreditPurchaseForFloatRequest.ShouldNotBeNull();
            recordCreditPurchaseForFloatRequest.CostPrice.ShouldBe(TestData.FloatCreditCostPrice);
            recordCreditPurchaseForFloatRequest.CreditAmount.ShouldBe(TestData.FloatCreditAmount);
            recordCreditPurchaseForFloatRequest.EstateId.ShouldBe(TestData.EstateId);
            recordCreditPurchaseForFloatRequest.FloatId.ShouldBe(TestData.FloatAggregateId);
            recordCreditPurchaseForFloatRequest.PurchaseDateTime.ShouldBe(TestData.CreditPurchasedDateTime);

        }

        #endregion
    }
}