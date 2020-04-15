using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.TransactionAggregate.Tests
{
    using Models;
    using Shouldly;
    using Testing;
    using Transaction.DomainEvents;
    using Xunit;

    public class DomainEventTests
    {
        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionHasStartedEvent_CanBeCreated_IsCreated(TransactionType transactionType)
        {
            TransactionHasStartedEvent transactionHasStartedEvent = TransactionHasStartedEvent.Create(TestData.TransactionId,
                                                                                                      TestData.EstateId,
                                                                                                      TestData.MerchantId,
                                                                                                      TestData.TransactionDateTime,
                                                                                                      TestData.TransactionNumber,
                                                                                                      transactionType.ToString(),
                                                                                                      TestData.TransactionReference,
                                                                                                      TestData.DeviceIdentifier);
            transactionHasStartedEvent.ShouldNotBeNull();
            transactionHasStartedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionHasStartedEvent.EventCreatedDateTime.ShouldNotBe(DateTime.MinValue);
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
                TransactionHasBeenLocallyAuthorisedEvent.Create(TestData.TransactionId,
                                                                TestData.EstateId,
                                                                TestData.MerchantId,
                                                                TestData.AuthorisationCode,
                                                                TestData.ResponseCode,
                                                                TestData.ResponseMessage);

            transactionHasBeenLocallyAuthorisedEvent.ShouldNotBeNull();
            transactionHasBeenLocallyAuthorisedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionHasBeenLocallyAuthorisedEvent.EventCreatedDateTime.ShouldNotBe(DateTime.MinValue);
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
            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = TransactionHasBeenCompletedEvent.Create(TestData.TransactionId,
                                                                                                                        TestData.EstateId,
                                                                                                                        TestData.MerchantId,
                                                                                                                        TestData.ResponseCode,
                                                                                                                        TestData.ResponseMessage,
                                                                                                                        TestData.IsAuthorised);

            transactionHasBeenCompletedEvent.ShouldNotBeNull();
            transactionHasBeenCompletedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionHasBeenCompletedEvent.EventCreatedDateTime.ShouldNotBe(DateTime.MinValue);
            transactionHasBeenCompletedEvent.EventId.ShouldNotBe(Guid.Empty);
            transactionHasBeenCompletedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            transactionHasBeenCompletedEvent.EstateId.ShouldBe(TestData.EstateId);
            transactionHasBeenCompletedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            transactionHasBeenCompletedEvent.ResponseCode.ShouldBe(TestData.ResponseCode);
            transactionHasBeenCompletedEvent.ResponseMessage.ShouldBe(TestData.ResponseMessage);
            transactionHasBeenCompletedEvent.IsAuthorised.ShouldBe(TestData.IsAuthorised);
        }

        [Fact]
        public void TransactionHasBeenLocallyDeclinedEvent_CanBeCreated_IsCreated()
        {
            TransactionHasBeenLocallyDeclinedEvent transactionHasBeenLocallyDeclinedEvent = TransactionHasBeenLocallyDeclinedEvent.Create(TestData.TransactionId,
                                                                                                                                          TestData.EstateId,
                                                                                                                                          TestData.MerchantId,
                                                                                                                                          TestData.DeclinedResponseCode,
                                                                                                                                          TestData
                                                                                                                                              .DeclinedResponseMessage);

            transactionHasBeenLocallyDeclinedEvent.ShouldNotBeNull();
            transactionHasBeenLocallyDeclinedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionHasBeenLocallyDeclinedEvent.EventCreatedDateTime.ShouldNotBe(DateTime.MinValue);
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
            TransactionAuthorisedByOperatorEvent transactionAuthorisedByOperatorEvent = TransactionAuthorisedByOperatorEvent.Create(TestData.TransactionId,
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
            transactionAuthorisedByOperatorEvent.EventCreatedDateTime.ShouldNotBe(DateTime.MinValue);
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
            TransactionDeclinedByOperatorEvent transactionDeclinedByOperatorEvent = TransactionDeclinedByOperatorEvent.Create(TestData.TransactionId,
                                                                                                                              TestData.EstateId,
                                                                                                                              TestData.MerchantId,
                                                                                                                              TestData.OperatorIdentifier1,
                                                                                                                              TestData.DeclinedOperatorResponseCode,
                                                                                                                              TestData.DeclinedOperatorResponseMessage,
                                                                                                                              TestData.DeclinedResponseCode,
                                                                                                                              TestData.DeclinedResponseMessage);

            transactionDeclinedByOperatorEvent.ShouldNotBeNull();
            transactionDeclinedByOperatorEvent.AggregateId.ShouldBe(TestData.TransactionId);
            transactionDeclinedByOperatorEvent.EventCreatedDateTime.ShouldNotBe(DateTime.MinValue);
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
            AdditionalResponseDataRecordedEvent additionalResponseDataRecordedEvent = AdditionalResponseDataRecordedEvent.Create(TestData.TransactionId,
                                                                                                                                 TestData.EstateId,
                                                                                                                                 TestData.MerchantId,
                                                                                                                                 TestData.OperatorIdentifier1,
                                                                                                                                 TestData.AdditionalTransactionMetaData);

            additionalResponseDataRecordedEvent.ShouldNotBeNull();
            additionalResponseDataRecordedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            additionalResponseDataRecordedEvent.EventCreatedDateTime.ShouldNotBe(DateTime.MinValue);
            additionalResponseDataRecordedEvent.EventId.ShouldNotBe(Guid.Empty);
            additionalResponseDataRecordedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            additionalResponseDataRecordedEvent.EstateId.ShouldBe(TestData.EstateId);
            additionalResponseDataRecordedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            additionalResponseDataRecordedEvent.OperatorIdentifier.ShouldBe(TestData.OperatorIdentifier1);
            additionalResponseDataRecordedEvent.AdditionalTransactionResponseMetadata.ShouldNotBeNull();

            foreach (KeyValuePair<String, String> keyValuePair in TestData.AdditionalTransactionMetaData)
            {
                additionalResponseDataRecordedEvent.AdditionalTransactionResponseMetadata.ShouldContainKeyAndValue(keyValuePair.Key, keyValuePair.Value);
            }
        }

        [Fact]
        public void AdditionalRequestDataRecordedEvent_CanBeCreated_IsCreated()
        {
            AdditionalRequestDataRecordedEvent additionalRequestDataRecordedEvent = AdditionalRequestDataRecordedEvent.Create(TestData.TransactionId,
                                                                                                                              TestData.EstateId,
                                                                                                                              TestData.MerchantId,
                                                                                                                              TestData.OperatorIdentifier1,
                                                                                                                              TestData.AdditionalTransactionMetaData);

            additionalRequestDataRecordedEvent.ShouldNotBeNull();
            additionalRequestDataRecordedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            additionalRequestDataRecordedEvent.EventCreatedDateTime.ShouldNotBe(DateTime.MinValue);
            additionalRequestDataRecordedEvent.EventId.ShouldNotBe(Guid.Empty);
            additionalRequestDataRecordedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            additionalRequestDataRecordedEvent.EstateId.ShouldBe(TestData.EstateId);
            additionalRequestDataRecordedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            additionalRequestDataRecordedEvent.OperatorIdentifier.ShouldBe(TestData.OperatorIdentifier1);
            additionalRequestDataRecordedEvent.AdditionalTransactionRequestMetadata.ShouldNotBeNull();

            foreach (KeyValuePair<String, String> keyValuePair in TestData.AdditionalTransactionMetaData)
            {
                additionalRequestDataRecordedEvent.AdditionalTransactionRequestMetadata.ShouldContainKeyAndValue(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
