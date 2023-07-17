using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.SettlementAggregates.Tests
{
    using Models;
    using Settlement.DomainEvents;
    using Shouldly;
    using Testing;
    using Xunit;

    public class DomainEventTests
    {
        [Fact]
        public void SettlementCreatedForDateEvent_CanBeCreated_IsCreated()
        {
            SettlementCreatedForDateEvent settlementCreatedForDateEvent =
                new SettlementCreatedForDateEvent(TestData.SettlementAggregateId, TestData.EstateId,TestData.MerchantId, TestData.SettlementDate);

            settlementCreatedForDateEvent.ShouldNotBeNull();
            settlementCreatedForDateEvent.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            settlementCreatedForDateEvent.EventId.ShouldNotBe(Guid.Empty);
            settlementCreatedForDateEvent.SettlementId.ShouldBe(TestData.SettlementAggregateId);
            settlementCreatedForDateEvent.EstateId.ShouldBe(TestData.EstateId);
            settlementCreatedForDateEvent.MerchantId.ShouldBe(TestData.MerchantId);
            settlementCreatedForDateEvent.SettlementDate.ShouldBe(TestData.SettlementDate);
        }

        [Fact]
        public void MerchantFeeAddedPendingSettlementEvent_CanBeCreated_IsCreated()
        {
            MerchantFeeAddedPendingSettlementEvent merchantFeeAddedPendingSettlementEvent =
                new MerchantFeeAddedPendingSettlementEvent(TestData.SettlementAggregateId,
                                                           TestData.EstateId,
                                                           TestData.MerchantId,
                                                           TestData.TransactionId,
                                                           TestData.CalculatedFeeValue,
                                                           (Int32)CalculationType.Fixed,
                                                           TestData.TransactionFeeId,
                                                           TestData.TransactionFeeValue,
                                                           TestData.TransactionFeeCalculateDateTime);

            merchantFeeAddedPendingSettlementEvent.ShouldNotBeNull();
            merchantFeeAddedPendingSettlementEvent.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            merchantFeeAddedPendingSettlementEvent.EventId.ShouldNotBe(Guid.Empty);
            merchantFeeAddedPendingSettlementEvent.SettlementId.ShouldBe(TestData.SettlementAggregateId);
            merchantFeeAddedPendingSettlementEvent.EstateId.ShouldBe(TestData.EstateId);
            merchantFeeAddedPendingSettlementEvent.MerchantId.ShouldBe(TestData.MerchantId);
            merchantFeeAddedPendingSettlementEvent.TransactionId.ShouldBe(TestData.TransactionId);
            merchantFeeAddedPendingSettlementEvent.CalculatedValue.ShouldBe(TestData.CalculatedFeeValue);
            merchantFeeAddedPendingSettlementEvent.FeeCalculationType.ShouldBe((Int32)CalculationType.Fixed);
            merchantFeeAddedPendingSettlementEvent.FeeId.ShouldBe(TestData.TransactionFeeId);
            merchantFeeAddedPendingSettlementEvent.FeeValue.ShouldBe(TestData.TransactionFeeValue);
            merchantFeeAddedPendingSettlementEvent.FeeCalculatedDateTime.ShouldBe(TestData.TransactionFeeCalculateDateTime);
        }

        [Fact]
        public void MerchantFeeSettledEvent_CanBeCreated_IsCreated()
        {
            MerchantFeeSettledEvent merchantFeeSettledEvent =
                new MerchantFeeSettledEvent(TestData.SettlementAggregateId, TestData.EstateId, TestData.MerchantId,
                                            TestData.TransactionId, TestData.CalculatedFeeValue,
                                            (Int32)CalculationType.Fixed,
                                            TestData.TransactionFeeId,
                                            TestData.TransactionFeeValue,
                                            TestData.TransactionFeeCalculateDateTime);

            merchantFeeSettledEvent.ShouldNotBeNull();
            merchantFeeSettledEvent.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            merchantFeeSettledEvent.EventId.ShouldNotBe(Guid.Empty);
            merchantFeeSettledEvent.SettlementId.ShouldBe(TestData.SettlementAggregateId);
            merchantFeeSettledEvent.EstateId.ShouldBe(TestData.EstateId);
            merchantFeeSettledEvent.MerchantId.ShouldBe(TestData.MerchantId);
            merchantFeeSettledEvent.TransactionId.ShouldBe(TestData.TransactionId);
            merchantFeeSettledEvent.CalculatedValue.ShouldBe(TestData.CalculatedFeeValue);
            merchantFeeSettledEvent.FeeCalculationType.ShouldBe((Int32)CalculationType.Fixed);
            merchantFeeSettledEvent.FeeId.ShouldBe(TestData.TransactionFeeId);
            merchantFeeSettledEvent.FeeValue.ShouldBe(TestData.TransactionFeeValue);
            merchantFeeSettledEvent.FeeCalculatedDateTime.ShouldBe(TestData.TransactionFeeCalculateDateTime);
        }

        [Fact]
        public void SettlementCompletedEvent_CanBeCreated_IsCreated()
        {
            SettlementCompletedEvent settlementCompletedEvent =
                new SettlementCompletedEvent(TestData.SettlementAggregateId, TestData.EstateId);

            settlementCompletedEvent.ShouldNotBeNull();
            settlementCompletedEvent.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            settlementCompletedEvent.EventId.ShouldNotBe(Guid.Empty);
            settlementCompletedEvent.SettlementId.ShouldBe(TestData.SettlementAggregateId);
            settlementCompletedEvent.EstateId.ShouldBe(TestData.EstateId);
        }
    }
}
