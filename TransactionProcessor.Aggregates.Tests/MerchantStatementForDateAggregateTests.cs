using Shouldly;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.Models.Merchant;
using TransactionProcessor.Testing;
using MerchantDepositSource = TransactionProcessor.Models.Merchant.MerchantDepositSource;

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
        Result result = merchantStatementForDateAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
            TestData.StatementDate,
            TestData.EventId1,
            TestData.EstateId,
            TestData.MerchantId, TestData.Transaction1);
        result.IsSuccess.ShouldBeTrue();

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

        Result result = merchantStatementForDateAggregate.AddTransactionToStatement(TestData.MerchantStatementId,
            TestData.StatementDate,
            TestData.EventId1,
            TestData.EstateId,
            TestData.MerchantId, TestData.Transaction1);
        result.IsSuccess.ShouldBeTrue();

        MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
        List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
        statementLines.ShouldNotBeNull();
        statementLines.ShouldNotBeEmpty();
        statementLines.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData("", null, null)]
    [InlineData(null, "", null)]
    [InlineData(null, null, "")]
    public void MerchantStatementForDateAggregate_AddTransactionToStatement_InvalidData_TransactionAddedToStatement(String statementId, String estateId, String merchantId) {
        Guid statementIdGuid = TestData.MerchantStatementId;
        Guid estateIdGuid = TestData.EstateId;
        Guid merchantIdGuid = TestData.MerchantId;

        if (statementId == String.Empty) {
            statementIdGuid = Guid.Empty;
        }
        if (estateId == String.Empty) {
            estateIdGuid = Guid.Empty;
        }
        if (merchantId == String.Empty) {
            merchantIdGuid = Guid.Empty;
        }
        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        Result result = merchantStatementForDateAggregate.AddTransactionToStatement(statementIdGuid,
            TestData.StatementDate,
            TestData.EventId1,
            estateIdGuid,
            merchantIdGuid, TestData.Transaction1);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public void MerchantStatementForDateAggregate_AddSettledFeeToStatement_FeeAddedToStatement()
    {
        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        Result result = merchantStatementForDateAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EventId1, TestData.EstateId, TestData.MerchantId, TestData.SettledFee1);
        result.IsSuccess.ShouldBeTrue();

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
        Result result = merchantStatementForDateAggregate.AddSettledFeeToStatement(TestData.MerchantStatementId, TestData.StatementDate, TestData.EventId1, TestData.EstateId, TestData.MerchantId, TestData.SettledFee1);
        result.IsSuccess.ShouldBeTrue();

        MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
        List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
        statementLines.ShouldNotBeNull();
        statementLines.ShouldNotBeEmpty();
        statementLines.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData("", null, null)]
    [InlineData(null, "", null)]
    [InlineData(null, null, "")]
    public void MerchantStatementForDateAggregate_AddSettledFeeToStatement_InvalidData_TransactionAddedToStatement(String statementId, String estateId, String merchantId)
    {
        Guid statementIdGuid = TestData.MerchantStatementId;
        Guid estateIdGuid = TestData.EstateId;
        Guid merchantIdGuid = TestData.MerchantId;

        if (statementId == String.Empty)
        {
            statementIdGuid = Guid.Empty;
        }
        if (estateId == String.Empty)
        {
            estateIdGuid = Guid.Empty;
        }
        if (merchantId == String.Empty)
        {
            merchantIdGuid = Guid.Empty;
        }

        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        Result result = merchantStatementForDateAggregate.AddSettledFeeToStatement(statementIdGuid, TestData.StatementDate, TestData.EventId1, estateIdGuid, merchantIdGuid, TestData.SettledFee1);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Theory]
    [InlineData(MerchantDepositSource.Manual)]
    [InlineData(MerchantDepositSource.Automatic)]
    public void MerchantStatementForDateAggregate_AddDepositToStatement_DepositAddedToStatement(MerchantDepositSource source)
    {
        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        Result result = merchantStatementForDateAggregate.AddDepositToStatement(TestData.MerchantStatementId,
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
        result.IsSuccess.ShouldBeTrue();

        MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
        List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
        statementLines.ShouldNotBeNull();
        statementLines.ShouldNotBeEmpty();
        statementLines.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData(null, "", null)]
    [InlineData(null, null, "")]
    [InlineData("", null, null)]
    public void MerchantStatementForDateAggregate_AddDepositToStatement_InvalidData_TransactionAddedToStatement(String statementId, String estateId, String merchantId)
    {
        Guid statementIdGuid = TestData.MerchantStatementId;
        Guid estateIdGuid = TestData.EstateId;
        Guid merchantIdGuid = TestData.MerchantId;

        if (statementId == String.Empty)
        {
            statementIdGuid = Guid.Empty;
        }
        if (estateId == String.Empty)
        {
            estateIdGuid = Guid.Empty;
        }
        if (merchantId == String.Empty)
        {
            merchantIdGuid = Guid.Empty;
        }

        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        Result result = merchantStatementForDateAggregate.AddDepositToStatement(statementIdGuid,
            TestData.StatementDate,
            TestData.EventId1,
            estateIdGuid,
            merchantIdGuid, new Deposit
            {
                DepositDateTime = TestData.DepositDateTime,
                Amount = TestData.DepositAmount.Value,
                DepositId = TestData.DepositId,
                Reference = TestData.DepositReference,
                Source = MerchantDepositSource.Manual
            });

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public void MerchantStatementForDateAggregate_AddWithdrawalToStatement_WithdrawalAddedToStatement()
    {
        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        Result result = merchantStatementForDateAggregate.AddWithdrawalToStatement(TestData.MerchantStatementId,
            TestData.StatementDate,
            TestData.EventId1,
            TestData.EstateId,
            TestData.MerchantId, new Withdrawal
            {
                WithdrawalDateTime = TestData.WithdrawalDateTime,
                Amount = TestData.WithdrawalAmount.Value,
                WithdrawalId = TestData.WithdrawalId
            });
        result.IsSuccess.ShouldBeTrue();

        MerchantStatementForDate merchantStatementForDate = merchantStatementForDateAggregate.GetStatement(true);
        List<MerchantStatementLine>? statementLines = merchantStatementForDate.GetStatementLines();
        statementLines.ShouldNotBeNull();
        statementLines.ShouldNotBeEmpty();
        statementLines.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData("", null, null)]
    [InlineData(null, "", null)]
    [InlineData( null, null, "")]
    public void MerchantStatementForDateAggregate_AddWithdrawalToStatement_InvalidData_TransactionAddedToStatement(String statementId, String estateId, String merchantId)
    {
        Guid statementIdGuid = TestData.MerchantStatementId;
        Guid estateIdGuid = TestData.EstateId;
        Guid merchantIdGuid = TestData.MerchantId;

        if (statementId == String.Empty)
        {
            statementIdGuid = Guid.Empty;
        }
        if (estateId == String.Empty)
        {
            estateIdGuid = Guid.Empty;
        }
        if (merchantId == String.Empty)
        {
            merchantIdGuid = Guid.Empty;
        }

        MerchantStatementForDateAggregate merchantStatementForDateAggregate = MerchantStatementForDateAggregate.Create(TestData.MerchantStatementForDateId1);
        Result result = merchantStatementForDateAggregate.AddWithdrawalToStatement(statementIdGuid,
            TestData.StatementDate,
            TestData.EventId1,
            estateIdGuid,
            merchantIdGuid, new Withdrawal
            {
                WithdrawalDateTime = TestData.WithdrawalDateTime,
                Amount = TestData.WithdrawalAmount.Value,
                WithdrawalId = TestData.WithdrawalId
            });

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }
}