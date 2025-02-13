using Shouldly;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class MerchantStatementAggregateTests
    {
        [Fact]
        public void MerchantStatementAggregate_CanBeCreated_IsCreated()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);

            merchantStatementAggregate.ShouldNotBeNull();
            merchantStatementAggregate.AggregateId.ShouldBe(TestData.MerchantStatementId);
        }
        
        [Fact]
        public void MerchantStatementAggregate_AddTransactionToStatement_TransactionAddedToStatement()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                                                                 TestData.EventId1,
                                                                 TestData.StatementCreateDate,
                                                                 TestData.EstateId,
                                                                 TestData.MerchantId, TestData.Transaction1);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement(true);
            var statementLines = merchantStatement.GetStatementLines();
            statementLines.ShouldNotBeNull();
            statementLines.ShouldNotBeEmpty();
            statementLines.Count.ShouldBe(1);
        }

        [Fact]
        public void MerchantStatementAggregate_AddTransactionToStatement_DuplicateTransaction_SilentlyHandled()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                                                                 TestData.EventId1,
                                                                 TestData.StatementCreateDate,
                                                                 TestData.EstateId,
                                                                 TestData.MerchantId, TestData.Transaction1);

            merchantStatementAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
                                                                 TestData.EventId1,
                                                                 TestData.StatementCreateDate,
                                                                 TestData.EstateId,
                                                                 TestData.MerchantId, TestData.Transaction1);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement(true);
            var statementLines = merchantStatement.GetStatementLines();
            statementLines.ShouldNotBeNull();
            statementLines.ShouldNotBeEmpty();
            statementLines.Count.ShouldBe(1);
        }

        [Fact]
        public void MerchantStatementAggregate_AddSettledFeeToStatement_FeeAddedToStatement()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId,
                                                                TestData.EventId1,
                                                                TestData.StatementCreateDate,
                                                                TestData.EstateId,
                                                                TestData.MerchantId, TestData.SettledFee1);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement(true);
            var statementLines = merchantStatement.GetStatementLines();
            statementLines.ShouldNotBeNull();
            statementLines.ShouldNotBeEmpty();
            statementLines.Count.ShouldBe(1);
        }

        [Fact]
        public void MerchantStatementAggregate_AddSettledFeeToStatement_DuplicateFee_Silentlyhandled()
        {
            MerchantStatementAggregate merchantStatementAggregate = MerchantStatementAggregate.Create(TestData.MerchantStatementId);
            merchantStatementAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId,
                                                                TestData.EventId1,
                                                                TestData.StatementCreateDate,
                                                                TestData.EstateId,
                                                                TestData.MerchantId, TestData.SettledFee1);

            merchantStatementAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId,
                                                                TestData.EventId1,
                                                                TestData.StatementCreateDate,
                                                                TestData.EstateId,
                                                                TestData.MerchantId, TestData.SettledFee1);

            MerchantStatement merchantStatement = merchantStatementAggregate.GetStatement(true);
            var statementLines = merchantStatement.GetStatementLines();
            statementLines.ShouldNotBeNull();
            statementLines.ShouldNotBeEmpty();
            statementLines.Count.ShouldBe(1);
        }

        [Fact]
        public void MerchantStatementAggregate_GenerateStatement_StatementIsGenerated()
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

            var merchantStatement = merchantStatementAggregate.GetStatement();
            merchantStatement.IsGenerated.ShouldBeTrue();
        }

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
        }
    }
}
