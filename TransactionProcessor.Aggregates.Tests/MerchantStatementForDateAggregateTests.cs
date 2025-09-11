using Shouldly;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests;

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

    [Theory]
    [InlineData(MerchantDepositSource.Manual)]
    [InlineData(MerchantDepositSource.Automatic)]
    public void MerchantStatementForDateAggregate_AddDepositToStatement_DepositAddedToStatement(MerchantDepositSource source)
    {
        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        merchantStatementForDateAggregate.AddDepositToStatement(TestData.MerchantStatementId,
            TestData.StatementDate,
            TestData.EventId1,
            TestData.EstateId,
            TestData.MerchantId, new Deposit {
                DepositDateTime = TestData.DepositDateTime,
                Amount = TestData.DepositAmount.Value,
                DepositId = TestData.DepositId,
                Reference = TestData.DepositReference,
                Source = source
            });

        MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
        List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
        statementLines.ShouldNotBeNull();
        statementLines.ShouldNotBeEmpty();
        statementLines.Count.ShouldBe(1);
    }

    [Fact]
    public void MerchantStatementForDateAggregate_AddWithdrawalToStatement_WithdrawalAddedToStatement()
    {
        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        merchantStatementForDateAggregate.AddWithdrawalToStatement(TestData.MerchantStatementId,
            TestData.StatementDate,
            TestData.EventId1,
            TestData.EstateId,
            TestData.MerchantId, new Withdrawal
            {
                WithdrawalDateTime = TestData.WithdrawalDateTime,
                Amount = TestData.WithdrawalAmount.Value,
                WithdrawalId = TestData.WithdrawalId
            });

        MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
        List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
        statementLines.ShouldNotBeNull();
        statementLines.ShouldNotBeEmpty();
        statementLines.Count.ShouldBe(1);
    }    
}