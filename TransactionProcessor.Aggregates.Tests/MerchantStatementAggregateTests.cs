using Shouldly;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class MerchantStatementForDateAggregateTests
    {
        [Fact]
        public void MerchantStatementForDateAggregate_CanBeCreated_IsCreated()
        {
            MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);

            merchantStatementForDateAggregate.ShouldNotBeNull();
            merchantStatementForDateAggregate.AggregateId.ShouldBe(TestData.MerchantStatementForDateId1);
        }
        
        [Fact]
        public void MerchantStatementForDateAggregate_AddTransactionToStatement_TransactionAddedToStatement()
        {
            MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
            merchantStatementForDateAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                TestData.StatementDate,
                TestData.EventId1,
                TestData.EstateId,
                TestData.MerchantId, TestData.Transaction1);

            MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
            List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
            statementLines.ShouldNotBeNull();
            statementLines.ShouldNotBeEmpty();
            statementLines.Count.ShouldBe(1);
        }
        
        [Fact]
        public void MerchantStatementForDateAggregate_AddTransactionToStatement_DuplicateTransaction_SilentlyHandled()
        {
            MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
            merchantStatementForDateAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                TestData.StatementDate,
                TestData.EventId1,
                TestData.EstateId,
                TestData.MerchantId, TestData.Transaction1);

            merchantStatementForDateAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                TestData.StatementDate,
                TestData.EventId1,
                TestData.EstateId,
                TestData.MerchantId, TestData.Transaction1);

            MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
            List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
            statementLines.ShouldNotBeNull();
            statementLines.ShouldNotBeEmpty();
            statementLines.Count.ShouldBe(1);
        }

        [Fact]
        public void MerchantStatementForDateAggregate_AddSettledFeeToStatement_FeeAddedToStatement()
        {
            MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
            merchantStatementForDateAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EventId1, TestData.EstateId, TestData.MerchantId, TestData.SettledFee1);

            MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
            List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
            statementLines.ShouldNotBeNull();
            statementLines.ShouldNotBeEmpty();
            statementLines.Count.ShouldBe(1);
        }
        
        [Fact]
        public void MerchantStatementForDateAggregate_AddSettledFeeToStatement_DuplicateFee_Silentlyhandled()
        {
            MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
            merchantStatementForDateAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EventId1, TestData.EstateId, TestData.MerchantId, TestData.SettledFee1);
            merchantStatementForDateAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EventId1, TestData.EstateId, TestData.MerchantId, TestData.SettledFee1);

            MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
            List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
            statementLines.ShouldNotBeNull();
            statementLines.ShouldNotBeEmpty();
            statementLines.Count.ShouldBe(1);
        }

        [Fact]
        public void MerchantStatementAggregate_AddDailySummaryRecord_RecordIsAdded() {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            Should.NotThrow(() => { merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m); });
        }

        [Fact]
        public void MerchantStatementAggregate_AddDailySummaryRecord_DuplicateAdd_ExceptionIsThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);
            Should.Throw<InvalidOperationException>(() => { merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m); });
        }

        
        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_StatementIsGenerated()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.IsGenerated.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_StatementIsAlreadyGenerated_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            Should.Throw<InvalidOperationException>(() => {
                merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            });
        }

        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_NoSummaries_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            
            Should.Throw<InvalidOperationException>(() => {
                merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            });
        }

        [Fact]
        public void MerchantStatementAggregate_BuildStatement_StatementIsBuilt()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate,TestData.StatementData);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.BuiltDateTime.ShouldBe(TestData.StatementBuiltDate);
        }

        [Fact]
        public void MerchantStatementAggregate_BuildStatement_StatementIsAlreadyBuilt_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);

            Should.Throw<InvalidOperationException>(() => {
                merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);
            });
        }

        [Fact]
        public void MerchantStatementAggregate_BuildStatement_StatementIsNotGenerated_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);

            Should.Throw<InvalidOperationException>(() => {
                merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);
            });
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_StatementIsEmailed()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);

            merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.HasBeenEmailed.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_StatementIsAlreadyEmailed_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
            merchantStatementAggregate.BuildStatement(TestData.StatementBuiltDate, TestData.StatementData);
            merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);

            Should.Throw<InvalidOperationException>(() => {
                merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);
            });
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_StatementIsNotBuilt_ExceptionThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddDailySummaryRecord(TestData.TransactionDateTime.Date, 1, 100.00m, 1, 0.10m);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);

            Should.Throw<InvalidOperationException>(() => {
                merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);
            });
        }

        /*
        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_StatementNotCreated_ErrorThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
                                                    });
        }

        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_StatementAlreadyGenerated_ErrorThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                                                                 TestData.EventId1,
                                                                 TestData.StatementCreateDate,
                                                                 TestData.EstateId,
                                                                 TestData.MerchantId, TestData.Transaction1);
            merchantStatementAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId,
                                                                TestData.EventId1,
                                                                TestData.StatementCreateDate,
                                                                TestData.EstateId,
                                                                TestData.MerchantId, TestData.SettledFee1);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
                                                    });
        }

        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_StatementHasNoTransactionsOrSettledFees_ErrorThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);
                                                    });
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_StatementHasBeenEmailed()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                                                                 TestData.EventId1,
                                                                 TestData.StatementCreateDate,
                                                                 TestData.EstateId,
                                                                 TestData.MerchantId, TestData.Transaction1);
            merchantStatementAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId,
                                                                TestData.EventId1,
                                                                TestData.StatementCreateDate,
                                                                TestData.EstateId,
                                                                TestData.MerchantId,
                                                                TestData.SettledFee1);
            merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);

            merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);

            MerchantStatement statement = merchantStatementAggregate.GetStatement(false);
            statement.HasBeenEmailed.ShouldBeTrue();
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_NotCreated_ErrorThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            //merchantStatementAggregate.CreateStatement(TestData.EstateId, TestData.MerchantId, TestData.StatementCreateDate);
            //merchantStatementAggregate.AddTransactionToStatement(TestData.Transaction1);
            //merchantStatementAggregate.AddSettledFeeToStatement(TestData.SettledFee1);
            //merchantStatementAggregate.GenerateStatement(TestData.StatementGeneratedDate);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate,TestData.MessageId);
                                                    });
        }

        [Fact]
        public void MerchantStatementAggregate_EmailStatement_NotGenerated_ErrorThrown()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                                                                 TestData.EventId1,
                                                                 TestData.StatementCreateDate,
                                                                 TestData.EstateId,
                                                                 TestData.MerchantId, TestData.Transaction1);
            merchantStatementAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId,
                                                                TestData.EventId1,
                                                                TestData.StatementCreateDate,
                                                                TestData.EstateId,
                                                                TestData.MerchantId, TestData.SettledFee1);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        merchantStatementAggregate.EmailStatement(TestData.StatementEmailedDate, TestData.MessageId);
                                                    });
        }*/
    }

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
