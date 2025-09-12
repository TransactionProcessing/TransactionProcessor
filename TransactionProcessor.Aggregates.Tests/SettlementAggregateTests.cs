using Shouldly;
using SimpleResults;
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
            Result result  =  aggregate.Create(TestData.EstateId,TestData.MerchantId, TestData.SettlementDate);
            result.IsSuccess.ShouldBeTrue();

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

            Result result = aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void SettlementAggregate_AddFee_FeeIsAdded()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            
            aggregate.AggregateId.ShouldBe(TestData.SettlementAggregateId);
            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);
            var fee = aggregate.GetFeesToBeSettled().SingleOrDefault();
            fee.ShouldNotBe(default);
            fee.merchantId.ShouldBe(TestData.MerchantId);
            fee.transactionId.ShouldBe(TestData.TransactionId);
            fee.calculatedFee.CalculatedValue.ShouldBe(TestData.CalculatedFeeMerchantFee().CalculatedValue);
            fee.calculatedFee.FeeCalculationType.ShouldBe(TestData.CalculatedFeeMerchantFee().FeeCalculationType);
            fee.calculatedFee.FeeId.ShouldBe(TestData.CalculatedFeeMerchantFee().FeeId);
            fee.calculatedFee.FeeValue.ShouldBe(TestData.CalculatedFeeMerchantFee().FeeValue);
            fee.calculatedFee.FeeType.ShouldBe(TestData.CalculatedFeeMerchantFee().FeeType);
            fee.calculatedFee.FeeCalculatedDateTime.ShouldBe(TestData.CalculatedFeeMerchantFee().FeeCalculatedDateTime);
            fee.calculatedFee.IsSettled.ShouldBe(TestData.CalculatedFeeMerchantFee().IsSettled);
            fee.calculatedFee.SettlementDueDate.ShouldBe(TestData.CalculatedFeeMerchantFee().SettlementDueDate);
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
        public void SettlementAggregate_AddFee_AggregateNotCreated_ErrorThrown() {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);

            Result result = aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
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
            Result result = aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeServiceProviderFee());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void SettlementAggregate_AddFee_InvalidMerchantId_ErrorThrown() {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            
            Result result = aggregate.AddFee(Guid.Empty, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void SettlementAggregate_AddFee_InvalidTransactionId_ErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);

            Result result = aggregate.AddFee(TestData.MerchantId, Guid.Empty, TestData.CalculatedFeeMerchantFee());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void SettlementAggregate_AddFee_InvalidFee_ErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);

            Result result = aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, null);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_FeeIsSettledAndSettlementCompleted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            aggregate.AddFee(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee());

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);

            Result result = aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);
            result.IsSuccess.ShouldBeTrue();

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

            Result result = aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);
            result.IsSuccess.ShouldBeTrue();

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(1);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeFalse();
        }

        [Fact]
        public void SettlementAggregate_MarkFeeAsSettled_PendingFeeNotFound_NoErrorThrown()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);

            Result result = aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);
            result.IsSuccess.ShouldBeTrue();

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

            Result result = aggregate.MarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId, TestData.SettlementDate);
            result.IsSuccess.ShouldBeTrue();

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

            Result result = aggregate.ImmediatelyMarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);
            result.IsSuccess.ShouldBeTrue();

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

            Result result = aggregate.ImmediatelyMarkFeeAsSettled(TestData.MerchantId, TestData.TransactionId, TestData.CalculatedFeeMerchantFee().FeeId);
            result.IsSuccess.ShouldBeTrue();

            aggregate.GetNumberOfFeesPendingSettlement().ShouldBe(0);
            aggregate.GetNumberOfFeesSettled().ShouldBe(1);
            aggregate.SettlementComplete.ShouldBeFalse();
        }

        [Fact]
       public void SettlementAggregate_StartProcessing_ProcessingStarted()
        {
            SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
            aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
            Result result = aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTime);
            result.IsSuccess.ShouldBeTrue();

            aggregate.ProcessingStarted.ShouldBeTrue();
            aggregate.ProcessingStartedDateTime.ShouldBe(TestData.SettlementProcessingStartedDateTime);
        }

       [Fact]
       public void SettlementAggregate_StartProcessing_CalledTwice_ProcessingStarted(){
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);
           aggregate.Create(TestData.EstateId, TestData.MerchantId, TestData.SettlementDate);
           aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTime);
           Result result = aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTimeSecondCall);

           result.IsSuccess.ShouldBeTrue();
           
           aggregate.ProcessingStarted.ShouldBeTrue();
           aggregate.ProcessingStartedDateTime.ShouldBe(TestData.SettlementProcessingStartedDateTime);
       }

       [Fact]
       public void SettlementAggregate_StartProcessing_SettlementNotCreated_ErrorThron()
       {
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);

            Result result = aggregate.StartProcessing(TestData.SettlementProcessingStartedDateTime);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
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
       public void SettlementAggregate_ManuallyComplete_SettlementNotCreated_ErrorThron() {
           SettlementAggregate aggregate = SettlementAggregate.Create(TestData.SettlementAggregateId);

           Result result = aggregate.ManuallyComplete();
           result.IsFailed.ShouldBeTrue();
           result.Status.ShouldBe(ResultStatus.Invalid);
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