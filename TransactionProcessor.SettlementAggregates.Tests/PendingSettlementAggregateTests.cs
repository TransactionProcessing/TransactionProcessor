namespace TransactionProcessor.SettlementAggregates.Tests
{
    using System;
    using Shouldly;
    using Testing;
    using TransactionAggregate;
    using Xunit;

    public class PendingSettlementAggregateTests
    {
        [Fact]
        public void PendingSettlementAggregate_CanBeCreated_IsCreated()
        {
            PendingSettlementAggregate aggregate = PendingSettlementAggregate.Create(TestData.PendingSettlementAggregateId);

            aggregate.AggregateId.ShouldBe(TestData.PendingSettlementAggregateId);
        }

        [Fact]
        public void PendingSettlementAggregate_Create_AggregateIsCreated()
        {
            PendingSettlementAggregate aggregate = PendingSettlementAggregate.Create(TestData.PendingSettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.SettlmentDate.ShouldBe(TestData.SettlementDate.Date);
            aggregate.IsCreated.ShouldBeTrue();
        }

        [Fact]
        public void PendingSettlementAggregate_Create_AlreadyCreated_ErrorThrown()
        {
            PendingSettlementAggregate aggregate = PendingSettlementAggregate.Create(TestData.PendingSettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Create(TestData.EstateId, TestData.SettlementDate);
                                                    });
        }

        [Fact]
        public void PendingSettlementAggregate_AddFee_FeeIsAdded()
        {
            PendingSettlementAggregate aggregate = PendingSettlementAggregate.Create(TestData.PendingSettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee);
            
            aggregate.AggregateId.ShouldBe(TestData.PendingSettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);
        }
        
        [Fact]
        public void PendingSettlementAggregate_AddFee_TwoFeesAdded_FeesAreAdded()
        {
            PendingSettlementAggregate aggregate = PendingSettlementAggregate.Create(TestData.PendingSettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee2);

            aggregate.AggregateId.ShouldBe(TestData.PendingSettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(2);
        }

        [Fact]
        public void PendingSettlementAggregate_AddFee_TwoFeesAdded_SameFeeIdDifferentTransaction_FeesAreAdded()
        {
            PendingSettlementAggregate aggregate = PendingSettlementAggregate.Create(TestData.PendingSettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId2, TestData.CalculatedFeeMerchantFee);

            aggregate.AggregateId.ShouldBe(TestData.PendingSettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(2);
        }

        [Fact]
        public void PendingSettlementAggregate_AddFee_AggregateNotCreated_ErrorThrown()
        {
            PendingSettlementAggregate aggregate = PendingSettlementAggregate.Create(TestData.PendingSettlementAggregateId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.AddFee(TestData.MerchantId,
                                                                         TestData.TransactionId,
                                                                         TestData.CalculatedFeeMerchantFee);
                                                    });
        }

        [Fact]
        public void PendingSettlementAggregate_AddFee_DuplicateFee_ErrorThrown()
        {
            PendingSettlementAggregate aggregate = PendingSettlementAggregate.Create(TestData.PendingSettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.AddFee(TestData.MerchantId,
                                                                         TestData.TransactionId,
                                                                         TestData.CalculatedFeeMerchantFee);
                                                    });
        }
    }
}