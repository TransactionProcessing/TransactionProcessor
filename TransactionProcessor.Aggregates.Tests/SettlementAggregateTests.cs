using Shouldly;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
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
            aggregate.Create(TestData.EstateId,TestData.MerchantId, TestData.SettlementDate);
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.SettlementDate.ShouldBe(TestData.SettlementDate.Date);
            aggregate.IsCreated.ShouldBeTrue();
            aggregate.SettlementComplete.ShouldBeFalse();
        }

        [Fact]
        public void SettlementAggregate_Create_AlreadyCreated_ErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
                                                    });
        }

        [Fact]
        public void SettlementAggregate_AddFee_FeeIsAdded()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            
            aggregate.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);
        }
        
        [Fact]
        public void SettlementAggregate_AddFee_TwoFeesAdded_FeesAreAdded()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee2);

            aggregate.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(2);
        }

        [Fact]
        public void SettlementAggregate_AddFee_TwoFeesAdded_SameFeeIdDifferentTransaction_FeesAreAdded()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
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
        public void SettlementAggregate_AddFee_DuplicateFee_SettledFee_NoErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);
            Should.NotThrow(() =>
            {
                aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            });
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
        }

        [Fact]
        public void SettlementAggregate_AddFee_DuplicateFee_PendingSettlement_NoErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
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
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeServiceProviderFee());
                                                    });
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_FeeIsSettledAndSettlementCompleted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeTrue();
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_FeeIsSettled_SettlementNotCompleted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee2);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(2);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeFalse();
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_PendingFeeNotFound_NoErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(0);
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_FeeAlreadySettled_NoErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);

            aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);
        }

        [Fact]
        public void SettlementAggregate_ImmediatelyMarkFeeAsSettled_FeeIsSettledAndSettlementNotCompleted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);

            aggregate.ImmediatelyMarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeFalse();
        }

        [Fact]
        public void SettlementAggregate_ImmediatelyMarkFeeAsSettled_FeeIsAlreadySettled()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);

            aggregate.ImmediatelyMarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);
            
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeFalse();
        }

        [Fact]
       public void SettlementAggregate_StartProcessing_ProcessingStarted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTime);

            aggregate.ProcessingStarted.ShouldBeTrue();
            aggregate.ProcessingStartedDateTime.ShouldBe(TestData.SettlementProcessingStartedDateTime);
        }

       [Fact]
       public void SettlementAggregate_StartProcessing_CalledTwice_ProcessingStarted(){
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
           aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
           aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTime);
           aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTimeSecondCall);

            aggregate.ProcessingStarted.ShouldBeTrue();
            aggregate.ProcessingStartedDateTime.ShouldBe(TestData.SettlementProcessingStartedDateTimeSecondCall);
       }

       [Fact]
       public void SettlementAggregate_StartProcessing_SettlementNotCreated_ErrorThron()
       {
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);

           Should.Throw<InvalidOperationException>(() => {
                            aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTime);
                        });
       }

       [Fact]
       public void SettlementAggregate_ManuallyComplete_SettlementCompleted()
       {
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
           aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
           aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTime);
           aggregate.ManuallyComplete();

            aggregate.SettlementComplete.ShouldBeTrue();
       }

       [Fact]
       public void SettlementAggregate_ManuallyComplete_CalledTwice_SettlementCompleted()
       {
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
           aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
           aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTime);
           aggregate.ManuallyComplete();
           aggregate.ManuallyComplete();

            aggregate.SettlementComplete.ShouldBeTrue();
       }

       [Fact]
       public void SettlementAggregate_ManuallyComplete_SettlementNotCreated_ErrorThron()
       {
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);

           Should.Throw<InvalidOperationException>(() => {
                                                       aggregate.ManuallyComplete();
                                                   });
       }

       [Fact]
       public void SettlementAggregate_GetFeesToBeSettled_ListIsEmpty()
       {
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
           aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
           var fees = aggregate.GetFeesToBeSettled();
           fees.ShouldBeEmpty();
       }
    }
}