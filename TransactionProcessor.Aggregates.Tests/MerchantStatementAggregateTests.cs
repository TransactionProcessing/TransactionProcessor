using Shouldly;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class MerchantStatementAggregateTests {
        [Fact]
        public void MerchantStatementAggregate_CanBeCreated_IsCreated()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);

            merchantStatementAggregate.ShouldNotBeNull();
            merchantStatementAggregate.AggregateId.ShouldBe(TestData.MerchantStatementId);
        }

        [Fact]
        public void MerchantStatementAggregate_RecordActivityDateOnStatement_ActivityDateIsRecorded() {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId,
                TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId1,
                TestData.ActivityDate1);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.IsCreated.ShouldBe(merchantStatement.IsCreated);
            List<(Guid merchantStatementForDateId, DateTime activityDate)>? activityDates = merchantStatement.GetActivityDates();
            activityDates.ShouldNotBeNull();
            activityDates.ShouldNotBeEmpty();
            activityDates.Count.ShouldBe(1);
        }

        [Fact]
        public void MerchantStatementAggregate_RecordActivityDateOnStatement_DuplicateActivityDate_Silentlyhandled()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId,
                TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId1,
                TestData.ActivityDate1);

            merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId,
                TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId1,
                TestData.ActivityDate1);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.IsCreated.ShouldBe(merchantStatement.IsCreated);
            List<(Guid merchantStatementForDateId, DateTime activityDate)>? activityDates = merchantStatement.GetActivityDates();
            activityDates.ShouldNotBeNull();
            activityDates.ShouldNotBeEmpty();
            activityDates.Count.ShouldBe(1);
        }
    }
}
