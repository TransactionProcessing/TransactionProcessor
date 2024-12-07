using Shouldly;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.FloatAggregate.Tests;

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
        aggregate.RecordCreditPurchase(TestData.EstateId, TestData.CreditPurchasedDateTime, TestData.FloatCreditAmount);
    }

    [Fact]
    public void FloatActivityAggregate_RecordTransactionAgainstFloat_TransactionRecorded()
    {
        FloatActivityAggregate aggregate = FloatActivityAggregate.Create(TestData.FloatAggregateId);
        aggregate.RecordTransactionAgainstFloat(TestData.EstateId, TestData.TransactionDateTime, TestData.TransactionAmount);
    }
}