using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.TransactionAggregate.Tests
{
    using EstateManagement.DataTransferObjects;
    using Models;
    using Shouldly;
    using Testing;
    using Transaction.DomainEvents;
    using Xunit;
    using CalculationType = Models.CalculationType;

    public class DomainEventTests
    {
        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionHasStartedEvent_CanBeCreated_IsCreated(TransactionType transactionType)
        {
            TransactionHasStartedEvent transactionHasStartedEvent = new TransactionHasStartedEvent(TestData.TransactionId,
                                                                                                      TestData.EstateId,
                                                                                                      TestData.MerchantId,
                                                                                                      TestData.TransactionDateTime,
                                                                                                      TestData.TransactionNumber,
                                                                                                      transactionType.ToString(),
                                                                                                      TestData.TransactionReference,
                                                                                                      TestData.DeviceIdentifier,
                                                                                                      TestData.TransactionAmount);
            transactionHasStartedEvent.ShouldNotBeNull();
            transactionHasStartedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionHasStartedEvent.TransactionAmount.ShouldBe(TestData.TransactionAmount);
            transactionHasStartedEvent.EventId.ShouldNotBe(Guid.Empty);
            transactionHasStartedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            transactionHasStartedEvent.EstateId.ShouldBe(TestData.EstateId);
            transactionHasStartedEvent.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
            transactionHasStartedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            transactionHasStartedEvent.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            transactionHasStartedEvent.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            transactionHasStartedEvent.TransactionType.ShouldBe(transactionType.ToString());
            transactionHasStartedEvent.TransactionReference.ShouldBe(TestData.TransactionReference);
        }

        [Fact]
        public void TransactionHasBeenLocallyAuthorisedEvent_CanBeCreated_IsCreated()
        {
            TransactionHasBeenLocallyAuthorisedEvent transactionHasBeenLocallyAuthorisedEvent =
                new TransactionHasBeenLocallyAuthorisedEvent(TestData.TransactionId,
                                                                TestData.EstateId,
                                                                TestData.MerchantId,
                                                                TestData.AuthorisationCode,
                                                                TestData.ResponseCode,
                                                                TestData.ResponseMessage);

            transactionHasBeenLocallyAuthorisedEvent.ShouldNotBeNull();
            transactionHasBeenLocallyAuthorisedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionHasBeenLocallyAuthorisedEvent.EventId.ShouldNotBe(Guid.Empty);
            transactionHasBeenLocallyAuthorisedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            transactionHasBeenLocallyAuthorisedEvent.EstateId.ShouldBe(TestData.EstateId);
            transactionHasBeenLocallyAuthorisedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            transactionHasBeenLocallyAuthorisedEvent.AuthorisationCode.ShouldBe(TestData.AuthorisationCode);
            transactionHasBeenLocallyAuthorisedEvent.ResponseCode.ShouldBe(TestData.ResponseCode);
            transactionHasBeenLocallyAuthorisedEvent.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

        [Fact]
        public void TransactionHasBeenCompletedEvent_CanBeCreated_IsCreated()
        {
            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = new TransactionHasBeenCompletedEvent(TestData.TransactionId,
                                                                                                                        TestData.EstateId,
                                                                                                                        TestData.MerchantId,
                                                                                                                        TestData.ResponseCode,
                                                                                                                        TestData.ResponseMessage,
                                                                                                                        TestData.IsAuthorised, 
                                                                                                                        TestData.TransactionDateTime,
                                                                                                                        TestData.TransactionAmount);

            transactionHasBeenCompletedEvent.ShouldNotBeNull();
            transactionHasBeenCompletedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionHasBeenCompletedEvent.EventId.ShouldNotBe(Guid.Empty);
            transactionHasBeenCompletedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            transactionHasBeenCompletedEvent.EstateId.ShouldBe(TestData.EstateId);
            transactionHasBeenCompletedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            transactionHasBeenCompletedEvent.ResponseCode.ShouldBe(TestData.ResponseCode);
            transactionHasBeenCompletedEvent.ResponseMessage.ShouldBe(TestData.ResponseMessage);
            transactionHasBeenCompletedEvent.IsAuthorised.ShouldBe(TestData.IsAuthorised);
            transactionHasBeenCompletedEvent.CompletedDateTime.ShouldBe(TestData.TransactionDateTime);
            transactionHasBeenCompletedEvent.TransactionAmount.ShouldBe(TestData.TransactionAmount);
        }

        [Fact]
        public void TransactionHasBeenLocallyDeclinedEvent_CanBeCreated_IsCreated()
        {
            TransactionHasBeenLocallyDeclinedEvent transactionHasBeenLocallyDeclinedEvent = new TransactionHasBeenLocallyDeclinedEvent(TestData.TransactionId,
                                                                                                                                          TestData.EstateId,
                                                                                                                                          TestData.MerchantId,
                                                                                                                                          TestData.DeclinedResponseCode,
                                                                                                                                          TestData
                                                                                                                                              .DeclinedResponseMessage);

            transactionHasBeenLocallyDeclinedEvent.ShouldNotBeNull();
            transactionHasBeenLocallyDeclinedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionHasBeenLocallyDeclinedEvent.EventId.ShouldNotBe(Guid.Empty);
            transactionHasBeenLocallyDeclinedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            transactionHasBeenLocallyDeclinedEvent.EstateId.ShouldBe(TestData.EstateId);
            transactionHasBeenLocallyDeclinedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            transactionHasBeenLocallyDeclinedEvent.ResponseCode.ShouldBe(TestData.DeclinedResponseCode);
            transactionHasBeenLocallyDeclinedEvent.ResponseMessage.ShouldBe(TestData.DeclinedResponseMessage);
        }

        [Fact]
        public void TransactionAuthorisedByOperatorEvent_CanBeCreated_IsCreated()
        {
            TransactionAuthorisedByOperatorEvent transactionAuthorisedByOperatorEvent = new TransactionAuthorisedByOperatorEvent(TestData.TransactionId,
                                                                                                                                    TestData.EstateId,
                                                                                                                                    TestData.MerchantId,
                                                                                                                                    TestData.OperatorIdentifier1,
                                                                                                                                    TestData.OperatorAuthorisationCode,
                                                                                                                                    TestData.OperatorResponseCode,
                                                                                                                                    TestData.OperatorResponseMessage,
                                                                                                                                    TestData.OperatorTransactionId,
                                                                                                                                    TestData.ResponseCode,
                                                                                                                                    TestData.ResponseMessage);

            transactionAuthorisedByOperatorEvent.ShouldNotBeNull();
            transactionAuthorisedByOperatorEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionAuthorisedByOperatorEvent.EventId.ShouldNotBe(Guid.Empty);
            transactionAuthorisedByOperatorEvent.TransactionId.ShouldBe(TestData.TransactionId);
            transactionAuthorisedByOperatorEvent.EstateId.ShouldBe(TestData.EstateId);
            transactionAuthorisedByOperatorEvent.MerchantId.ShouldBe(TestData.MerchantId);
            transactionAuthorisedByOperatorEvent.AuthorisationCode.ShouldBe(TestData.OperatorAuthorisationCode);
            transactionAuthorisedByOperatorEvent.OperatorResponseCode.ShouldBe(TestData.OperatorResponseCode);
            transactionAuthorisedByOperatorEvent.OperatorResponseMessage.ShouldBe(TestData.OperatorResponseMessage);
            transactionAuthorisedByOperatorEvent.OperatorTransactionId.ShouldBe(TestData.OperatorTransactionId);
            transactionAuthorisedByOperatorEvent.ResponseCode.ShouldBe(TestData.ResponseCode);
            transactionAuthorisedByOperatorEvent.ResponseMessage.ShouldBe(TestData.ResponseMessage);
            transactionAuthorisedByOperatorEvent.OperatorIdentifier.ShouldBe(TestData.OperatorIdentifier1);
        }

        [Fact]
        public void TransactionDeclinedByOperatorEvent_CanBeCreated_IsCreated()
        {
            TransactionDeclinedByOperatorEvent transactionDeclinedByOperatorEvent = new TransactionDeclinedByOperatorEvent(TestData.TransactionId,
                                                                                                                              TestData.EstateId,
                                                                                                                              TestData.MerchantId,
                                                                                                                              TestData.OperatorIdentifier1,
                                                                                                                              TestData.DeclinedOperatorResponseCode,
                                                                                                                              TestData.DeclinedOperatorResponseMessage,
                                                                                                                              TestData.DeclinedResponseCode,
                                                                                                                              TestData.DeclinedResponseMessage);

            transactionDeclinedByOperatorEvent.ShouldNotBeNull();
            transactionDeclinedByOperatorEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionDeclinedByOperatorEvent.EventId.ShouldNotBe(Guid.Empty);
            transactionDeclinedByOperatorEvent.TransactionId.ShouldBe(TestData.TransactionId);
            transactionDeclinedByOperatorEvent.EstateId.ShouldBe(TestData.EstateId);
            transactionDeclinedByOperatorEvent.MerchantId.ShouldBe(TestData.MerchantId);
            transactionDeclinedByOperatorEvent.OperatorResponseCode.ShouldBe(TestData.DeclinedOperatorResponseCode);
            transactionDeclinedByOperatorEvent.OperatorResponseMessage.ShouldBe(TestData.DeclinedOperatorResponseMessage);
            transactionDeclinedByOperatorEvent.ResponseCode.ShouldBe(TestData.DeclinedResponseCode);
            transactionDeclinedByOperatorEvent.ResponseMessage.ShouldBe(TestData.DeclinedResponseMessage);
            transactionDeclinedByOperatorEvent.OperatorIdentifier.ShouldBe(TestData.OperatorIdentifier1);
        }

        [Fact]
        public void AdditionalResponseDataRecordedEvent_CanBeCreated_IsCreated()
        {
            AdditionalResponseDataRecordedEvent additionalResponseDataRecordedEvent = new AdditionalResponseDataRecordedEvent(TestData.TransactionId,
                                                                                                                                 TestData.EstateId,
                                                                                                                                 TestData.MerchantId,
                                                                                                                                 TestData.OperatorIdentifier1,
                                                                                                                                 TestData.AdditionalTransactionMetaDataForMobileTopup());

            additionalResponseDataRecordedEvent.ShouldNotBeNull();
            additionalResponseDataRecordedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            additionalResponseDataRecordedEvent.EventId.ShouldNotBe(Guid.Empty);
            additionalResponseDataRecordedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            additionalResponseDataRecordedEvent.EstateId.ShouldBe(TestData.EstateId);
            additionalResponseDataRecordedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            additionalResponseDataRecordedEvent.OperatorIdentifier.ShouldBe(TestData.OperatorIdentifier1);
            additionalResponseDataRecordedEvent.AdditionalTransactionResponseMetadata.ShouldNotBeNull();

            foreach (KeyValuePair<String, String> keyValuePair in TestData.AdditionalTransactionMetaDataForMobileTopup())
            {
                additionalResponseDataRecordedEvent.AdditionalTransactionResponseMetadata.ShouldContainKeyAndValue(keyValuePair.Key, keyValuePair.Value);
            }
        }

        [Fact]
        public void AdditionalRequestDataRecordedEvent_CanBeCreated_IsCreated()
        {
            AdditionalRequestDataRecordedEvent additionalRequestDataRecordedEvent = new AdditionalRequestDataRecordedEvent(TestData.TransactionId,
                                                                                                                              TestData.EstateId,
                                                                                                                              TestData.MerchantId,
                                                                                                                              TestData.OperatorIdentifier1,
                                                                                                                              TestData.AdditionalTransactionMetaDataForMobileTopup());

            additionalRequestDataRecordedEvent.ShouldNotBeNull();
            additionalRequestDataRecordedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            additionalRequestDataRecordedEvent.EventId.ShouldNotBe(Guid.Empty);
            additionalRequestDataRecordedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            additionalRequestDataRecordedEvent.EstateId.ShouldBe(TestData.EstateId);
            additionalRequestDataRecordedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            additionalRequestDataRecordedEvent.OperatorIdentifier.ShouldBe(TestData.OperatorIdentifier1);
            additionalRequestDataRecordedEvent.AdditionalTransactionRequestMetadata.ShouldNotBeNull();

            foreach (KeyValuePair<String, String> keyValuePair in TestData.AdditionalTransactionMetaDataForMobileTopup())
            {
                additionalRequestDataRecordedEvent.AdditionalTransactionRequestMetadata.ShouldContainKeyAndValue(keyValuePair.Key, keyValuePair.Value);
            }
        }

        [Fact]
        public void CustomerEmailReceiptRequestedEvent_CanBeCreated_IsCreated()
        {
            CustomerEmailReceiptRequestedEvent customerEmailReceiptRequestedEvent = new CustomerEmailReceiptRequestedEvent(TestData.TransactionId,
                                                                                                                              TestData.EstateId,
                                                                                                                              TestData.MerchantId,
                                                                                                                              TestData.CustomerEmailAddress);

            customerEmailReceiptRequestedEvent.ShouldNotBeNull();
            customerEmailReceiptRequestedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            customerEmailReceiptRequestedEvent.EventId.ShouldNotBe(Guid.Empty);
            customerEmailReceiptRequestedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            customerEmailReceiptRequestedEvent.EstateId.ShouldBe(TestData.EstateId);
            customerEmailReceiptRequestedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            customerEmailReceiptRequestedEvent.CustomerEmailAddress.ShouldBe(TestData.CustomerEmailAddress);
        }

        [Fact]
        public void CustomerEmailReceiptResendRequestedEvent_CanBeCreated_IsCreated()
        {
            CustomerEmailReceiptResendRequestedEvent customerEmailReceiptResendRequestedEvent = new CustomerEmailReceiptResendRequestedEvent(TestData.TransactionId,
                 TestData.EstateId,
                 TestData.MerchantId);

            customerEmailReceiptResendRequestedEvent.ShouldNotBeNull();
            customerEmailReceiptResendRequestedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            customerEmailReceiptResendRequestedEvent.EventId.ShouldNotBe(Guid.Empty);
            customerEmailReceiptResendRequestedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            customerEmailReceiptResendRequestedEvent.EstateId.ShouldBe(TestData.EstateId);
            customerEmailReceiptResendRequestedEvent.MerchantId.ShouldBe(TestData.MerchantId);
        }

        [Fact]
        public void ProductDetailsAddedToTransactionEvent_CanBeCreated_IsCreated()
        {
            ProductDetailsAddedToTransactionEvent productDetailsAddedToTransactionEvent = new ProductDetailsAddedToTransactionEvent(TestData.TransactionId, TestData.EstateId,TestData.MerchantId, TestData.ContractId, TestData.ProductId);

            productDetailsAddedToTransactionEvent.ShouldNotBeNull();
            productDetailsAddedToTransactionEvent.AggregateId.ShouldBe(TestData.TransactionId);
            productDetailsAddedToTransactionEvent.EventId.ShouldNotBe(Guid.Empty);
            productDetailsAddedToTransactionEvent.TransactionId.ShouldBe(TestData.TransactionId);
            productDetailsAddedToTransactionEvent.EstateId.ShouldBe(TestData.EstateId);
            productDetailsAddedToTransactionEvent.MerchantId.ShouldBe(TestData.MerchantId);
            productDetailsAddedToTransactionEvent.ProductId.ShouldBe(TestData.ProductId);
            productDetailsAddedToTransactionEvent.ContractId.ShouldBe(TestData.ContractId);
        }

        [Fact]
        public void SettledMerchantFeeAddedToTransactionEvent_CanBeCreated_IsCreated()
        {
            SettledMerchantFeeAddedToTransactionEvent settledMerchantFeeAddedToTransactionEvent = new SettledMerchantFeeAddedToTransactionEvent(TestData.TransactionId,
                                                                                                                                         TestData.EstateId,
                                                                                                                                         TestData.MerchantId,
                                                                                                                                         TestData.CalculatedFeeValue,
                                                                                                                                         (Int32)CalculationType.Fixed,
                                                                                                                                         TestData.TransactionFeeId,
                                                                                                                                         TestData.TransactionFeeValue,
                                                                                                                                         TestData.TransactionFeeCalculateDateTime,
                                                                                                                                         TestData.TransactionFeeSettledDateTime,
                                                                                                                                                TestData.SettlementAggregateId);

            settledMerchantFeeAddedToTransactionEvent.ShouldNotBeNull();
            settledMerchantFeeAddedToTransactionEvent.AggregateId.ShouldBe(TestData.TransactionId);
            settledMerchantFeeAddedToTransactionEvent.EventId.ShouldNotBe(Guid.Empty);
            settledMerchantFeeAddedToTransactionEvent.TransactionId.ShouldBe(TestData.TransactionId);
            settledMerchantFeeAddedToTransactionEvent.EstateId.ShouldBe(TestData.EstateId);
            settledMerchantFeeAddedToTransactionEvent.MerchantId.ShouldBe(TestData.MerchantId);
            settledMerchantFeeAddedToTransactionEvent.CalculatedValue.ShouldBe(TestData.CalculatedFeeValue);
            settledMerchantFeeAddedToTransactionEvent.FeeCalculationType.ShouldBe((Int32)CalculationType.Fixed);
            settledMerchantFeeAddedToTransactionEvent.FeeId.ShouldBe(TestData.TransactionFeeId);
            settledMerchantFeeAddedToTransactionEvent.FeeValue.ShouldBe(TestData.TransactionFeeValue);
            settledMerchantFeeAddedToTransactionEvent.FeeCalculatedDateTime.ShouldBe(TestData.TransactionFeeCalculateDateTime);
            settledMerchantFeeAddedToTransactionEvent.SettledDateTime.ShouldBe(TestData.TransactionFeeSettledDateTime);

        }

        [Fact]
        public void ServiceProviderFeeAddedToTransactionEvent_CanBeCreated_IsCreated()
        {
            ServiceProviderFeeAddedToTransactionEvent serviceProviderFeeAddedToTransactionEvent = new ServiceProviderFeeAddedToTransactionEvent(TestData.TransactionId,
                                                                                                                                                   TestData.EstateId,
                                                                                                                                                   TestData.MerchantId,
                                                                                                                                                   TestData.CalculatedFeeValue,
                                                                                                                                                   (Int32)CalculationType.Fixed,
                                                                                                                                                   TestData.TransactionFeeId,
                                                                                                                                                   TestData.TransactionFeeValue,
                                                                                                                                                   TestData.TransactionFeeCalculateDateTime);

            serviceProviderFeeAddedToTransactionEvent.ShouldNotBeNull();
            serviceProviderFeeAddedToTransactionEvent.AggregateId.ShouldBe(TestData.TransactionId);
            serviceProviderFeeAddedToTransactionEvent.EventId.ShouldNotBe(Guid.Empty);
            serviceProviderFeeAddedToTransactionEvent.TransactionId.ShouldBe(TestData.TransactionId);
            serviceProviderFeeAddedToTransactionEvent.EstateId.ShouldBe(TestData.EstateId);
            serviceProviderFeeAddedToTransactionEvent.MerchantId.ShouldBe(TestData.MerchantId);
            serviceProviderFeeAddedToTransactionEvent.CalculatedValue.ShouldBe(TestData.CalculatedFeeValue);
            serviceProviderFeeAddedToTransactionEvent.FeeCalculationType.ShouldBe((Int32)CalculationType.Fixed);
            serviceProviderFeeAddedToTransactionEvent.FeeId.ShouldBe(TestData.TransactionFeeId);
            serviceProviderFeeAddedToTransactionEvent.FeeValue.ShouldBe(TestData.TransactionFeeValue);
            serviceProviderFeeAddedToTransactionEvent.FeeCalculatedDateTime.ShouldBe(TestData.TransactionFeeCalculateDateTime);
        }

        [Fact]
        public void TransactionSourceAddedToTransactionEvent_CanBeCreated_IsCreated()
        {
            TransactionSourceAddedToTransactionEvent transactionSourceAddedToTransactionEvent = new TransactionSourceAddedToTransactionEvent(TestData.TransactionId,
                                                                                                                                                   TestData.EstateId,
                                                                                                                                                   TestData.MerchantId,
                                                                                                                                                   TestData.TransactionSource);

            transactionSourceAddedToTransactionEvent.ShouldNotBeNull();
            transactionSourceAddedToTransactionEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionSourceAddedToTransactionEvent.EventId.ShouldNotBe(Guid.Empty);
            transactionSourceAddedToTransactionEvent.TransactionId.ShouldBe(TestData.TransactionId);
            transactionSourceAddedToTransactionEvent.EstateId.ShouldBe(TestData.EstateId);
            transactionSourceAddedToTransactionEvent.MerchantId.ShouldBe(TestData.MerchantId);
            transactionSourceAddedToTransactionEvent.TransactionSource.ShouldBe(TestData.TransactionSource);
        }

        [Fact]
        public void MerchantFeePendingSettlementAddedToTransactionEvent_CanBeCreated_IsCreated()
        {
            MerchantFeePendingSettlementAddedToTransactionEvent merchantFeePendingSettlementAddedToTransactionEvent = new MerchantFeePendingSettlementAddedToTransactionEvent(TestData.TransactionId,
                                                                                                                                                                   TestData.EstateId,
                                                                                                                                                                   TestData.MerchantId,
                                                                                                                                                                   TestData.CalculatedFeeValue,
                                                                                                                                                                   (Int32)CalculationType.Fixed,
                                                                                                                                                                   TestData.TransactionFeeId,
                                                                                                                                                                   TestData.TransactionFeeValue,
                                                                                                                                                                   TestData.TransactionFeeCalculateDateTime,
                                                                                                                                                                   TestData.TransactionFeeSettlementDueDate);

            merchantFeePendingSettlementAddedToTransactionEvent.ShouldNotBeNull();
            merchantFeePendingSettlementAddedToTransactionEvent.AggregateId.ShouldBe(TestData.TransactionId);
            merchantFeePendingSettlementAddedToTransactionEvent.EventId.ShouldNotBe(Guid.Empty);
            merchantFeePendingSettlementAddedToTransactionEvent.TransactionId.ShouldBe(TestData.TransactionId);
            merchantFeePendingSettlementAddedToTransactionEvent.EstateId.ShouldBe(TestData.EstateId);
            merchantFeePendingSettlementAddedToTransactionEvent.MerchantId.ShouldBe(TestData.MerchantId);
            merchantFeePendingSettlementAddedToTransactionEvent.CalculatedValue.ShouldBe(TestData.CalculatedFeeValue);
            merchantFeePendingSettlementAddedToTransactionEvent.FeeCalculationType.ShouldBe((Int32)CalculationType.Fixed);
            merchantFeePendingSettlementAddedToTransactionEvent.FeeId.ShouldBe(TestData.TransactionFeeId);
            merchantFeePendingSettlementAddedToTransactionEvent.FeeValue.ShouldBe(TestData.TransactionFeeValue);
            merchantFeePendingSettlementAddedToTransactionEvent.FeeCalculatedDateTime.ShouldBe(TestData.TransactionFeeCalculateDateTime);
            merchantFeePendingSettlementAddedToTransactionEvent.SettlementDueDate.ShouldBe(TestData.TransactionFeeSettlementDueDate);
        }
    }
}
