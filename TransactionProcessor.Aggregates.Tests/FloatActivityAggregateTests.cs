using Shouldly;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests;

public class FloatActivityAggregateTests {
    [Fact]
    public void FloatActivityAggregate_CanBeCreated_IsCreated()
    {
        FloatActivityAggregate aggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);

        aggregate.AggregateId.ShouldBe(TestData.FloatAggregateId);
    }

    [Fact]
    public void FloatActivityAggregate_RecordCreditPurchase_PurchaseRecorded() {
        FloatActivityAggregate aggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
        aggregate.RecordCreditPurchase(TestData.EstateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
        aggregate.CreditCount.ShouldBe(1);
        aggregate.Credits.Contains(TestData.FloatCreditId).ShouldBeTrue();
    }

    [Fact]
    public void FloatActivityAggregate_RecordCreditPurchase_DuplicateCredit_PurchaseNotRecorded()
    {
        FloatActivityAggregate aggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
        aggregate.RecordCreditPurchase(TestData.EstateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
        aggregate.CreditCount.ShouldBe(1);
        aggregate.RecordCreditPurchase(TestData.EstateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount, TestData.FloatCreditId);
        aggregate.CreditCount.ShouldBe(1);
    }

    [Fact]
    public void FloatActivityAggregate_RecordTransactionAgainstFloat_TransactionRecorded()
    {
        FloatActivityAggregate aggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
        aggregate.RecordTransactionAgainstFloat(TestData.EstateId, TestData.TransactionDateTime, TestData.TransactionAmount, TestData.TransactionId);
        aggregate.DebitCount.ShouldBe(1);
        aggregate.Debits.Contains(TestData.TransactionId).ShouldBeTrue();
    }

    [Fact]
    public void FloatActivityAggregate_RecordTransactionAgainstFloat_DuplicateTransaction_TransactionNotRecorded()
    {
        FloatActivityAggregate aggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
        aggregate.RecordTransactionAgainstFloat(TestData.EstateId, TestData.TransactionDateTime, TestData.TransactionAmount, TestData.TransactionId);
        aggregate.DebitCount.ShouldBe(1);
        aggregate.RecordTransactionAgainstFloat(TestData.EstateId, TestData.TransactionDateTime, TestData.TransactionAmount, TestData.TransactionId);
        aggregate.DebitCount.ShouldBe(1);
    }
}