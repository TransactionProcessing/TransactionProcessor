using Shouldly;
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

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            reconciliationAggregate.IsStarted.ShouldBeTrue();
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
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            DateTime transactionDateTime = validDateTime ? TestData.TransactionDateTime : DateTime.MinValue;
            Guid estateId = validEstateId ? TestData.EstateId : Guid.Empty;
            Guid merchantId = validMerchantId ? TestData.MerchantId : Guid.Empty;

            Should.Throw<ArgumentException>(() => { reconciliationAggregate.StartReconciliation(transactionDateTime, estateId, merchantId); });
        }

        [Fact]
        public void ReconciliationAggregate_StartReconciliation_ReconciliationAlreadyStarted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_StartReconciliation_ReconciliationAlreadyCompleted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            reconciliationAggregate.CompleteReconciliation();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
                                                    });
        }


        [Fact]
        public void ReconciliationAggregate_RecordOverallTotals_OverallTotalsAreRecorded()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            
            reconciliationAggregate.TransactionCount.ShouldBe(TestData.ReconciliationTransactionCount);
            reconciliationAggregate.TransactionValue.ShouldBe(TestData.ReconciliationTransactionValue);
        }

        [Fact]
        public void ReconciliationAggregate_RecordOverallTotals_ReconciliationNotStarted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount,
                                                                                                    TestData.ReconciliationTransactionValue);
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_RecordOverallTotals_ReconciliationAlreadyCompleted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            reconciliationAggregate.CompleteReconciliation();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount,
                                                                                                    TestData.ReconciliationTransactionValue);
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationIsAuthorised()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);

            reconciliationAggregate.ResponseCode.ShouldBe(TestData.ResponseCode);
            reconciliationAggregate.ResponseMessage.ShouldBe(TestData.ResponseMessage);
            reconciliationAggregate.IsAuthorised.ShouldBeTrue();
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationNotStarted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationAlreadyAuthorised_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationAlreadyDeclined_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_Authorise_ReconciliationAlreadyCompleted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
            reconciliationAggregate.CompleteReconciliation();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationIsDeclined()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);

            reconciliationAggregate.ResponseCode.ShouldBe(TestData.ResponseCode);
            reconciliationAggregate.ResponseMessage.ShouldBe(TestData.ResponseMessage);
            reconciliationAggregate.IsAuthorised.ShouldBeFalse();
        }

        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationNotStarted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
            {
                reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            });
        }

        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationAlreadyAuthorised_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Authorise(TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationAlreadyDecline_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
            {
                reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            });
        }
        
        [Fact]
        public void ReconciliationAggregate_Decline_ReconciliationAlreadyCompleted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);
            reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            reconciliationAggregate.CompleteReconciliation();

            Should.Throw<InvalidOperationException>(() =>
            {
                reconciliationAggregate.Decline(TestData.ResponseCode, TestData.ResponseMessage);
            });
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

            reconciliationAggregate.CompleteReconciliation();

            reconciliationAggregate.IsAuthorised.ShouldBe(isAuthorised);
            reconciliationAggregate.IsCompleted.ShouldBeTrue();
            reconciliationAggregate.IsStarted.ShouldBeFalse();
        }

        [Fact]
        public void ReconciliationAggregate_CompleteReconciliation_ReconciliationNotStarted_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.CompleteReconciliation();
                                                    });
        }

        [Fact]
        public void ReconciliationAggregate_CompleteReconciliation_NotAuthorisedOrDeclined_ErrorThrown()
        {
            Aggregates.ReconciliationAggregate reconciliationAggregate = Aggregates.ReconciliationAggregate.Create(TestData.TransactionId);

            reconciliationAggregate.StartReconciliation(TestData.TransactionDateTime, TestData.EstateId, TestData.MerchantId);
            reconciliationAggregate.RecordOverallTotals(TestData.ReconciliationTransactionCount, TestData.ReconciliationTransactionValue);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.CompleteReconciliation();
                                                    });
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

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        reconciliationAggregate.CompleteReconciliation();
                                                    });
        }
    }
}
