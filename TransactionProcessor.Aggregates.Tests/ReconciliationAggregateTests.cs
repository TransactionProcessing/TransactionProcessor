using Shouldly;
using SimpleResults;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class ReconciliationAggregateTests
    {
        [Fact]
        public void ReconciliationAggregate_CanBeCreated_IsCreated()
        {
            Aggregates.ReconciliationAggregate aggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            aggregate.AggregateId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public void ReconciliationAggregate_StartReconciliation_ReconciliationIsStarted()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            Result result = reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            result.IsSuccess.ShouldBeTrue();

            reconciliationAggregate.HasBeenStarted.ShouldBeTrue();
            reconciliationAggregate.EstateId.ShouldBe(TestData.EstateId);
            reconciliationAggregate.MerchantId.ShouldBe(TestData.MerchantId);
        }

        [Theory]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        public void ReconciliationAggregate_StartReconciliation_InvalidData_ErrorThrown(Boolean validDateTime,
                                                                                        Boolean validEstateId,
                                                                                        Boolean validMerchantId)
        {
            ReconciliationAggregate reconciliationAggregate = ReconciliationAggregate.Create(TestData.TransactionId);

            DateTime transactionDateTime = validDateTime ? TestData.TransactionDateTime : DateTime.MinValue;
            Guid estateId = validEstateId ? TestData.EstateId : Guid.Empty;
            Guid merchantId = validMerchantId ? TestData.MerchantId : Guid.Empty;

            Result result = reconciliationAggregate.StartReconciliation(transactionDateTime, estateId, merchantId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void ReconciliationAggregate_StartReconciliation_ReconciliationAlreadyStarted_NoErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            Result result = reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_StartReconciliation_ReconciliationAlreadyCompleted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            reconciliationAggregate.CompleteReconciliation();

            Result result = reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_RecordOverallTotals_OverallTotalsAreRecorded()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            Result result = reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            result.IsSuccess.ShouldBeTrue();

            reconciliationAggregate.TransactionCount.ShouldBe(TestData.ReconciliationTransactionCount);
            reconciliationAggregate.TransactionValue.ShouldBe(TestData.ReconciliationTransactionValue);
        }

        [Fact]
        public void ReconciliationAggregate_RecordOverallTotals_ReconciliationNotStarted_ErrorThrown()
        {
            ReconciliationAggregate reconciliationAggregate = ReconciliationAggregate.Create(TestData.TransactionId);

            Result result = reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount,
                                                                                                    TestData.ReconciliationTransactionValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void ReconciliationAggregate_RecordOverallTotals_ReconciliationAlreadyCompleted_ErrorThrown()
        {
            ReconciliationAggregate reconciliationAggregate = ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            reconciliationAggregate.CompleteReconciliation();

            Result result = reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount,
                                                                                                    TestData.ReconciliationTransactionValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationIsAuthorised()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            Result result = reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();

            reconciliationAggregate.ResponseCode.ShouldBe(TestData.ResponseCode.ToCodeString());
            reconciliationAggregate.ResponseMessage.ShouldBe(TestData.ResponseMessage);
            reconciliationAggregate.IsAuthorised.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationNotStarted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            Result result = reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationAlreadyAuthorised_NoErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);

            Result result = reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationAlreadyDeclined_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);

            Result result = reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationAlreadyCompleted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            reconciliationAggregate.CompleteReconciliation();

            Result result = reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationIsDeclined()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            Result result = reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();

            reconciliationAggregate.ResponseCode.ShouldBe(TestData.ResponseCode.ToCodeString());
            reconciliationAggregate.ResponseMessage.ShouldBe(TestData.ResponseMessage);
            reconciliationAggregate.IsAuthorised.ShouldBeFalse();
        }

        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationNotStarted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            Result result = reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationAlreadyAuthorised_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);

            Result result = reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationAlreadyDecline_NoErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);

            Result result = reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationAlreadyCompleted_NoErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            reconciliationAggregate.CompleteReconciliation();

            Result result = reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReconciliationAggregate_CompleteReconciliation_ReconciliationIsCompleted(Boolean isAuthorised)
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            if (isAuthorised)
            {
                reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            }

            Result result = reconciliationAggregate.CompleteReconciliation();
            result.IsSuccess.ShouldBeTrue();

            reconciliationAggregate.IsAuthorised.ShouldBe(isAuthorised);
            reconciliationAggregate.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_CompleteReconciliation_ReconciliationNotStarted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            Result result = reconciliationAggregate.CompleteReconciliation();
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void ReconciliationAggregate_CompleteReconciliation_NotAuthorisedOrDeclined_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            Result result = reconciliationAggregate.CompleteReconciliation();
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ReconciliationAggregate_CompleteReconciliation_ReconciliationAlreadyCompleted(Boolean isAuthorised)
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            if (isAuthorised)
            {
                reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            }

            reconciliationAggregate.CompleteReconciliation();

            Result result = reconciliationAggregate.CompleteReconciliation();
            result.IsSuccess.ShouldBeTrue();                                                    
        }
    }
}
