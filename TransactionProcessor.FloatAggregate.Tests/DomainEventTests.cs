namespace TransactionProcessor.FloatAggregate.Tests;

using Float.DomainEvents;
using Shouldly;
using Testing;
using Xunit;

public class DomainEventTests{
        
    [Fact]
    public void FloatCreatedForContractProductEvent_CanBeCreated_IsCreated()
    {
        FloatCreatedForContractProductEvent floatCreatedForContractProductEvent =
            new FloatCreatedForContractProductEvent(TestData.FloatAggregateId, TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);

        floatCreatedForContractProductEvent.ShouldNotBeNull();
        floatCreatedForContractProductEvent.AggregateId.ShouldBe(TestData.FloatAggregateId);
        floatCreatedForContractProductEvent.EventId.ShouldNotBe(Guid.Empty);
        floatCreatedForContractProductEvent.FloatId.ShouldBe(TestData.FloatAggregateId);
        floatCreatedForContractProductEvent.EstateId.ShouldBe(TestData.EstateId);
        floatCreatedForContractProductEvent.ContractId.ShouldBe(TestData.ContractId);
        floatCreatedForContractProductEvent.ProductId.ShouldBe(TestData.ProductId);
        floatCreatedForContractProductEvent.CreatedDateTime.ShouldBe(TestData.FloatCreatedDateTime);
    }

    [Fact]
    public void FloatCreditPurchasedEvent_CanBeCreated_IsCreated(){
        FloatCreditPurchasedEvent floatCreditPurchasedEvent = new FloatCreditPurchasedEvent(TestData.FloatAggregateId,
                                                                                            TestData.EstateId,
                                                                                            TestData.CreditPurchasedDateTime,
                                                                                            TestData.FloatCreditAmount,
                                                                                            TestData.FloatCreditCostPrice);

        floatCreditPurchasedEvent.ShouldNotBeNull();
        floatCreditPurchasedEvent.AggregateId.ShouldBe(TestData.FloatAggregateId);
        floatCreditPurchasedEvent.EventId.ShouldNotBe(Guid.Empty);
        floatCreditPurchasedEvent.FloatId.ShouldBe(TestData.FloatAggregateId);
        floatCreditPurchasedEvent.EstateId.ShouldBe(TestData.EstateId);
        floatCreditPurchasedEvent.Amount.ShouldBe(TestData.FloatCreditAmount);
        floatCreditPurchasedEvent.CostPrice.ShouldBe(TestData.FloatCreditCostPrice);
        floatCreditPurchasedEvent.CreditPurchasedDateTime.ShouldBe(TestData.CreditPurchasedDateTime);
    }
}