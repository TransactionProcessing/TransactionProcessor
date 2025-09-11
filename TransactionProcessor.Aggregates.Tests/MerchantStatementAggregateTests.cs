using Shouldly;
using SimpleResults;
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

        [Fact]
        public void MerchantStatementAggregate_RecordActivityDateOnStatement_InvalidStatementId_ErrorReturned()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            Result result = merchantStatementAggregate.RecordActivityDateOnStatement(Guid.Empty,
                TestData.StatementDate, TestData.EstateId, TestData.MerchantId, TestData.MerchantStatementForDateId1,
                TestData.ActivityDate1);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantStatementAggregate_RecordActivityDateOnStatement_InvalidEstateId_ErrorReturned()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            Result result = merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId,
                TestData.StatementDate, Guid.Empty, TestData.MerchantId, TestData.MerchantStatementForDateId1,
                TestData.ActivityDate1);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantStatementAggregate_RecordActivityDateOnStatement_InvalidMerchantId_ErrorReturned()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            Result result = merchantStatementAggregate.RecordActivityDateOnStatement(TestData.MerchantStatementId,
                TestData.StatementDate, TestData.EstateId, Guid.Empty,  TestData.MerchantStatementForDateId1,
                TestData.ActivityDate1);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantStatementAggregate_AddDailySummaryRecord_RecordIsAdded()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            Result result = merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m,
                1, 1000, 1, 200);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_AddDailySummaryRecord_DuplicateAdd_ExceptionIsThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m,
                1, 1000, 1, 200);
            Result result = merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m,
                1, 1000, 1, 200);
            result.IsSuccess.ShouldBeTrue();
        }


        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_StatementIsGenerated()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m,
                1, 1000, 1, 200);
            Result result = merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            result.IsSuccess.ShouldBeTrue();

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.IsGenerated.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_StatementIsAlreadyGenerated_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m, 1, 1000, 1, 200);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);

            Result result = merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_NoSummaries_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);

            Result result = merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantStatementAggregate_BuildStatement_StatementIsBuilt()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m, 1, 1000, 1, 200);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            Result result = merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);
            result.IsSuccess.ShouldBeTrue();

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.BuiltDateTime.ShouldBe(TestData.StatementBuiltDate);
        }

        [Fact]
        public void MerchantStatementAggregate_BuildStatement_StatementIsAlreadyBuilt_NoErrorThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m, 1, 1000, 1, 200);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);

            Result result = merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_BuildStatement_StatementIsNotGenerated_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m, 1, 1000, 1, 200);

            Result result = merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_StatementIsEmailed()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m, 1, 1000, 1, 200);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);

            Result result = merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);
            result.IsSuccess.ShouldBeTrue();

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.HasBeenEmailed.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_StatementIsAlreadyEmailed_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m, 1, 1000, 1, 200);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);
            merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);

            Result result = merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_StatementIsNotBuilt_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m, 1, 1000, 1, 200);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);

            var result = merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
    }
}
