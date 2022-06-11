namespace TransactionProcessor.SettlementAggregates.Tests
{
    using System;
    using Shouldly;
    using Testing;
    using TransactionAggregate;
    using Xunit;

    public class SettlementAggregateTests
    {
        [Fact]
        public void SettlementAggregate_CanBeCreated_IsCreated()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);

            aggregate.AggregateId.ShouldBe(TestData.SettlementAggregateId);
        }

        [Fact]
        public void PendingSettlementAggregate_Create_AggregateIsCreated()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.SettlementDate.ShouldBe(TestData.SettlementDate.Date);
            aggregate.IsCreated.ShouldBeTrue();
            aggregate.SettlementComplete.ShouldBeFalse();
        }

        [Fact]
        public void SettlementAggregate_Create_AlreadyCreated_ErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Create(TestData.EstateId, TestData.SettlementDate);
                                                    });
        }

        [Fact]
        public void SettlementAggregate_AddFee_FeeIsAdded()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            
            aggregate.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);
        }
        
        [Fact]
        public void SettlementAggregate_AddFee_TwoFeesAdded_FeesAreAdded()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee2);

            aggregate.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(2);
        }

        [Fact]
        public void SettlementAggregate_AddFee_TwoFeesAdded_SameFeeIdDifferentTransaction_FeesAreAdded()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId2, TestData.CalculatedFeeMerchantFee());

            aggregate.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(2);
        }

        [Fact]
        public void SettlementAggregate_AddFee_AggregateNotCreated_ErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.AddFee(TestData.MerchantId,
                                                                         TestData.TransactionId,
                                                                         TestData.CalculatedFeeMerchantFee());
                                                    });
        }

        [Fact]
        public void SettlementAggregate_AddFee_DuplicateFee_NoErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            Should.NotThrow(() =>
                            {
                                aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
                            });
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);
        }

        [Fact]
        public void SettlementAggregate_AddFee_InvalidFeeType_ErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeServiceProviderFee);
                                                    });
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_FeeIsSettledAndSettlementCompleted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeTrue();
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_FeeIsSettled_SettlementNotCompleted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee2);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(2);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeFalse();
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_PendingFeeNotFound_NoErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(0);
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_FeeAlreadySettled_NoErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);
        }

        [Fact]
        public void SettlementAggregate_ImmediatelyMarkFeeAsSettled_FeeIsSettledAndSettlementNotCompleted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);

            aggregate.ImmediatelyMarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeFalse();
        }

    }
}