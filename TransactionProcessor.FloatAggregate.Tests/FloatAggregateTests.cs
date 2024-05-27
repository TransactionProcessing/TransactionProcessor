namespace TransactionProcessor.FloatAggregate.Tests
{
    using Shouldly;
    using Testing;
    using TransactionProcessor.Settlement.DomainEvents;
    using Xunit;

    public class FloatAggregateTests
    {
        [Fact]
        public void FloatAggregate_CanBeCreated_IsCreated()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);

            aggregate.AggregateId.ShouldBe(TestData.FloatAggregateId);
        }

        [Fact]
        public void FloatAggregate_CreateFloat_IsCreated()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);

            aggregate.AggregateId.ShouldBe(TestData.FloatAggregateId);
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.ContractId.ShouldBe(TestData.ContractId);
            aggregate.ProductId.ShouldBe(TestData.ProductId);
            aggregate.CreatedDateTime.ShouldBe(TestData.FloatCreatedDateTime);
        }

        [Fact]
        public void FloatAggregate_CreateFloat_FloatAlreadyCreated_ErrorThrown()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);

            Should.Throw<InvalidOperationException>(() => {
                                                        aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
                                                    });
        }

        [Fact]
        public void FloatAggregate_RecordCreditPurchase_CreditPurchaseIsRecorded()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            aggregate.RecordCreditPurchase(DateTime.Now, 1000, 900);

            aggregate.NumberOfCreditPurchases.ShouldBe(1);
            aggregate.TotalCostPrice.ShouldBe(900);
            aggregate.UnitCostPrice.ShouldBe(0.9m);
            aggregate.TotalCreditPurchases.ShouldBe(1000);
            aggregate.Balance.ShouldBe(1000);
        }

        [Fact]
        public void FloatAggregate_RecordCreditPurchase_FloatNotCreated_ErrorThrown()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            
            Should.Throw<InvalidOperationException>(() => {
                aggregate.RecordCreditPurchase(DateTime.Now, 1000, 900);
            });
        }

        [Fact]
        public void FloatAggregate_RecordCreditPurchase_MultipleCreditPurchases_AllCreditPurchasesAreRecorded()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            aggregate.RecordCreditPurchase(DateTime.Now, 1000, 900);

            aggregate.NumberOfCreditPurchases.ShouldBe(1);
            aggregate.TotalCostPrice.ShouldBe(900);
            aggregate.UnitCostPrice.ShouldBe(0.9m);
            aggregate.TotalCreditPurchases.ShouldBe(1000);
            aggregate.Balance.ShouldBe(1000);
            Decimal roundedUnitCostPrice= aggregate.GetUnitCostPrice();
            roundedUnitCostPrice.ShouldBe(0.900m);
            aggregate.RecordCreditPurchase(DateTime.Now, 2000, 1750);

            aggregate.NumberOfCreditPurchases.ShouldBe(2);
            aggregate.TotalCostPrice.ShouldBe(2650);
            aggregate.UnitCostPrice.ShouldBe(0.8833333333333333333333333333m);
            roundedUnitCostPrice = aggregate.GetUnitCostPrice();
            roundedUnitCostPrice.ShouldBe(0.8833m);
            aggregate.TotalCreditPurchases.ShouldBe(3000);
            aggregate.Balance.ShouldBe(3000);

            aggregate.RecordCreditPurchase(DateTime.Now, 20000, 16000);

            aggregate.NumberOfCreditPurchases.ShouldBe(3);
            aggregate.TotalCostPrice.ShouldBe(18650);
            aggregate.UnitCostPrice.ShouldBe(0.810869565217391304347826087m);
            roundedUnitCostPrice = aggregate.GetUnitCostPrice();
            roundedUnitCostPrice.ShouldBe(0.8109m);
            aggregate.TotalCreditPurchases.ShouldBe(23000);
            aggregate.Balance.ShouldBe(23000);
        }

        [Fact]
        public void FloatAggregate_RecordCreditPurchase_DuplicateCreditPurchase_ErrorThrown()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            DateTime purchaseDateTime = DateTime.Now;
            aggregate.RecordCreditPurchase(purchaseDateTime, 1000, 900);

            Should.Throw<InvalidOperationException>(() => {
                                aggregate.RecordCreditPurchase(purchaseDateTime, 1000, 900);
                            });
        }

        [Fact]
        public void FloatAggregate_RecordTransactionAgainstFloat_FloatBalanceDecremented_And_TransactionAdded(){
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            DateTime purchaseDateTime = DateTime.Now;
            aggregate.RecordCreditPurchase(purchaseDateTime, 1000, 900);

            aggregate.Balance.ShouldBe(1000);

            aggregate.RecordTransactionAgainstFloat(TestData.TransactionId, 100);

            aggregate.Balance.ShouldBe(900);
            aggregate.NumberOfTransactions.ShouldBe(1);
            aggregate.TotalTransactions.ShouldBe(100);
        }

        [Fact]
        public void FloatAggregate_RecordTransactionAgainstFloat_NoBalance_FloatBalanceDecremented_And_TransactionAdded()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            aggregate.Balance.ShouldBe(0);

            aggregate.RecordTransactionAgainstFloat(TestData.TransactionId, 100);

            aggregate.Balance.ShouldBe(-100);
            aggregate.NumberOfTransactions.ShouldBe(1);
            aggregate.TotalTransactions.ShouldBe(100);
        }

        [Fact]
        public void FloatAggregate_RecordTransactionAgainstFloat_FloatNotCreated_ErrorThrown()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);

            Should.Throw<InvalidOperationException>(() => {
                                                        aggregate.RecordTransactionAgainstFloat(TestData.TransactionId, 100);
                                                    });
        }

        [Fact]
        public void FloatAggregate_RecordTransactionAgainstFloat_DuplicateTransaction_NoErrorThrown()
        {
            FloatAggregate aggregate = FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            DateTime purchaseDateTime = DateTime.Now;
            aggregate.RecordCreditPurchase(purchaseDateTime, 1000, 900);
            aggregate.RecordTransactionAgainstFloat(TestData.TransactionId, 100);
            Should.NotThrow(() => {
                                                        aggregate.RecordTransactionAgainstFloat(TestData.TransactionId, 100);
                                                    });
        }
    }
}