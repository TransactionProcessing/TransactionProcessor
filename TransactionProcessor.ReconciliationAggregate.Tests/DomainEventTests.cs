namespace TransactionProcessor.ReconciliationAggregate.Tests
{
    using System;
    using Reconciliation.DomainEvents;
    using Shouldly;
    using Testing;
    using Transaction.DomainEvents;
    using Xunit;

    /// <summary>
    /// 
    /// </summary>
    public class DomainEventTests
    {
        #region Methods

        /// <summary>
        /// Overalls the totals recorded event can be created is created.
        /// </summary>
        [Fact]
        public void OverallTotalsRecordedEvent_CanBeCreated_IsCreated()
        {
            OverallTotalsRecordedEvent overallTotalsRecordedEvent = new OverallTotalsRecordedEvent(TestData.TransactionId,
                                                                                                      TestData.EstateId,
                                                                                                      TestData.MerchantId,
                                                                                                      TestData.ReconciliationTransactionCount,
                                                                                                      TestData.ReconciliationTransactionValue);

            overallTotalsRecordedEvent.ShouldNotBeNull();
            overallTotalsRecordedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            overallTotalsRecordedEvent.EventId.ShouldNotBe(Guid.Empty);
            overallTotalsRecordedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            overallTotalsRecordedEvent.EstateId.ShouldBe(TestData.EstateId);
            overallTotalsRecordedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            overallTotalsRecordedEvent.TransactionCount.ShouldBe(TestData.ReconciliationTransactionCount);
            overallTotalsRecordedEvent.TransactionValue.ShouldBe(TestData.ReconciliationTransactionValue);
        }

        /// <summary>
        /// Reconciliations the has been locally authorised event can be created is created.
        /// </summary>
        [Fact]
        public void ReconciliationHasBeenLocallyAuthorisedEvent_CanBeCreated_IsCreated()
        {
            ReconciliationHasBeenLocallyAuthorisedEvent reconciliationHasBeenLocallyAuthorisedEvent =
                new ReconciliationHasBeenLocallyAuthorisedEvent(TestData.TransactionId,
                                                                   TestData.EstateId,
                                                                   TestData.MerchantId,
                                                                   TestData.ResponseCode,
                                                                   TestData.ResponseMessage);

            reconciliationHasBeenLocallyAuthorisedEvent.ShouldNotBeNull();
            reconciliationHasBeenLocallyAuthorisedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            reconciliationHasBeenLocallyAuthorisedEvent.EventId.ShouldNotBe(Guid.Empty);
            reconciliationHasBeenLocallyAuthorisedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            reconciliationHasBeenLocallyAuthorisedEvent.EstateId.ShouldBe(TestData.EstateId);
            reconciliationHasBeenLocallyAuthorisedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            reconciliationHasBeenLocallyAuthorisedEvent.ResponseCode.ShouldBe(TestData.ResponseCode);
            reconciliationHasBeenLocallyAuthorisedEvent.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

        /// <summary>
        /// Reconciliations the has completed event can be created is created.
        /// </summary>
        [Fact]
        public void ReconciliationHasCompletedEvent_CanBeCreated_IsCreated()
        {
            ReconciliationHasCompletedEvent reconciliationHasCompletedEvent =
                new ReconciliationHasCompletedEvent(TestData.TransactionId, TestData.EstateId, TestData.MerchantId);

            reconciliationHasCompletedEvent.ShouldNotBeNull();
            reconciliationHasCompletedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            reconciliationHasCompletedEvent.EventId.ShouldNotBe(Guid.Empty);
            reconciliationHasCompletedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            reconciliationHasCompletedEvent.EstateId.ShouldBe(TestData.EstateId);
            reconciliationHasCompletedEvent.MerchantId.ShouldBe(TestData.MerchantId);
        }

        /// <summary>
        /// Reconciliations the has started event can be created is created.
        /// </summary>
        [Fact]
        public void ReconciliationHasStartedEvent_CanBeCreated_IsCreated()
        {
            ReconciliationHasStartedEvent reconciliationHasStartedEvent = new ReconciliationHasStartedEvent(TestData.TransactionId,
                                                                                                               TestData.EstateId,
                                                                                                               TestData.MerchantId,
                                                                                                               TestData.TransactionDateTime);
            reconciliationHasStartedEvent.ShouldNotBeNull();
            reconciliationHasStartedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            reconciliationHasStartedEvent.EventId.ShouldNotBe(Guid.Empty);
            reconciliationHasStartedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            reconciliationHasStartedEvent.EstateId.ShouldBe(TestData.EstateId);
            reconciliationHasStartedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            reconciliationHasStartedEvent.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
        }

        /// <summary>
        /// Transactions the has been locally declined event can be created is created.
        /// </summary>
        [Fact]
        public void TransactionHasBeenLocallyDeclinedEvent_CanBeCreated_IsCreated()
        {
            ReconciliationHasBeenLocallyDeclinedEvent reconciliationHasBeenLocallyDeclinedEvent = new ReconciliationHasBeenLocallyDeclinedEvent(TestData.TransactionId,
                TestData.EstateId,
                TestData.MerchantId,
                TestData.DeclinedResponseCode,
                TestData.DeclinedResponseMessage);

            reconciliationHasBeenLocallyDeclinedEvent.ShouldNotBeNull();
            reconciliationHasBeenLocallyDeclinedEvent.AggregateId.ShouldBe(TestData.TransactionId);
            reconciliationHasBeenLocallyDeclinedEvent.EventId.ShouldNotBe(Guid.Empty);
            reconciliationHasBeenLocallyDeclinedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            reconciliationHasBeenLocallyDeclinedEvent.EstateId.ShouldBe(TestData.EstateId);
            reconciliationHasBeenLocallyDeclinedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            reconciliationHasBeenLocallyDeclinedEvent.ResponseCode.ShouldBe(TestData.DeclinedResponseCode);
            reconciliationHasBeenLocallyDeclinedEvent.ResponseMessage.ShouldBe(TestData.DeclinedResponseMessage);
        }

        #endregion
    }
}