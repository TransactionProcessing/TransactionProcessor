using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.TransactionAggregate.Tests
{
    using Shouldly;
    using Testing;
    using Transaction.DomainEvents;
    using Xunit;

    public class DomainEventTests
    {
        [Fact]
        public void TransactionHasStartedEvent_CanBeCreated_IsCreated()
        {
            TransactionHasStartedEvent transactionHasStartedEvent = TransactionHasStartedEvent.Create(TestData.TransactionId,
                                                                                                      TestData.EstateId,
                                                                                                      TestData.MerchantId,
                                                                                                      TestData.TransactionDateTime,
                                                                                                      TestData.TransactionNumber,
                                                                                                      TestData.TransactionType,
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
            transactionHasStartedEvent.TransactionType.ShouldBe(TestData.TransactionType);
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

    }
}
