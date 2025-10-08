using Shouldly;
using SimpleResults;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class FloatAggregateTests
    {
        [Fact]
        public void FloatAggregate_CanBeCreated_IsCreated()
        {
            Aggregates.FloatAggregate aggregate = Aggregates.FloatAggregate.Create(TestData.FloatAggregateId);

            aggregate.AggregateId.ShouldBe(TestData.FloatAggregateId);
        }

        [Fact]
        public void FloatAggregate_CreateFloat_IsCreated()
        {
            Aggregates.FloatAggregate aggregate = Aggregates.FloatAggregate.Create(TestData.FloatAggregateId);
            Result result = aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            result.IsSuccess.ShouldBeTrue();

            aggregate.AggregateId.ShouldBe(TestData.FloatAggregateId);
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.ContractId.ShouldBe(TestData.ContractId);
            aggregate.ProductId.ShouldBe(TestData.ProductId);
            aggregate.CreatedDateTime.ShouldBe(TestData.FloatCreatedDateTime);
        }

        [Fact]
        public void FloatAggregate_CreateFloat_FloatAlreadyCreated_NoErrorThrown() {
            Aggregates.FloatAggregate aggregate = Aggregates.FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);

            Result result = aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void FloatAggregate_RecordCreditPurchase_CreditPurchaseIsRecorded()
        {
            Aggregates.FloatAggregate aggregate = Aggregates.FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            Result result = aggregate.RecordCreditPurchase(DateTime.Now, 1000, 900);
            result.IsSuccess.ShouldBeTrue();
            aggregate.NumberOfCreditPurchases.ShouldBe(1);
            aggregate.TotalCostPrice.ShouldBe(900);
            aggregate.UnitCostPrice.ShouldBe(0.9m);
            aggregate.TotalCreditPurchases.ShouldBe(1000);
        }

        [Fact]
        public void FloatAggregate_RecordCreditPurchase_FloatNotCreated_ErrorThrown()
        {
            Aggregates.FloatAggregate aggregate = Aggregates.FloatAggregate.Create(TestData.FloatAggregateId);

            Result result = aggregate.RecordCreditPurchase(DateTime.Now, 1000, 900);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            
        }

        [Fact]
        public void FloatAggregate_RecordCreditPurchase_MultipleCreditPurchases_AllCreditPurchasesAreRecorded()
        {
            Aggregates.FloatAggregate aggregate = Aggregates.FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            aggregate.RecordCreditPurchase(DateTime.Now, 1000, 900);

            aggregate.NumberOfCreditPurchases.ShouldBe(1);
            aggregate.TotalCostPrice.ShouldBe(900);
            aggregate.UnitCostPrice.ShouldBe(0.9m);
            aggregate.TotalCreditPurchases.ShouldBe(1000);

            Decimal roundedUnitCostPrice= aggregate.GetUnitCostPrice();
            roundedUnitCostPrice.ShouldBe(0.900m);
            aggregate.RecordCreditPurchase(DateTime.Now, 2000, 1750);

            aggregate.NumberOfCreditPurchases.ShouldBe(2);
            aggregate.TotalCostPrice.ShouldBe(2650);
            aggregate.UnitCostPrice.ShouldBe(0.8833333333333333333333333333m);
            roundedUnitCostPrice = aggregate.GetUnitCostPrice();
            roundedUnitCostPrice.ShouldBe(0.8833m);
            aggregate.TotalCreditPurchases.ShouldBe(3000);

            aggregate.RecordCreditPurchase(DateTime.Now, 20000, 16000);

            aggregate.NumberOfCreditPurchases.ShouldBe(3);
            aggregate.TotalCostPrice.ShouldBe(18650);
            aggregate.UnitCostPrice.ShouldBe(0.810869565217391304347826087m);
            roundedUnitCostPrice = aggregate.GetUnitCostPrice();
            roundedUnitCostPrice.ShouldBe(0.8109m);
            aggregate.TotalCreditPurchases.ShouldBe(23000);
        }

        [Fact]
        public void FloatAggregate_RecordCreditPurchase_DuplicateCreditPurchase_ErrorThrown()
        {
            Aggregates.FloatAggregate aggregate = Aggregates.FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            DateTime purchaseDateTime = DateTime.Now;
            aggregate.RecordCreditPurchase(purchaseDateTime, 1000, 900);

            Result result = aggregate.RecordCreditPurchase(purchaseDateTime, 1000, 900);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);

        }

        [Fact]
        public void FloatAggregate_GetTotalCostPrice_TotalCostReturned()
        {
            Aggregates.FloatAggregate aggregate = Aggregates.FloatAggregate.Create(TestData.FloatAggregateId);
            aggregate.CreateFloat(TestData.EstateId, TestData.ContractId, TestData.ProductId, TestData.FloatCreatedDateTime);
            DateTime purchaseDateTime = DateTime.Now;
            aggregate.RecordCreditPurchase(purchaseDateTime, 1000, 900);

            Decimal totalCost = aggregate.GetTotalCostPrice(10);

            totalCost.ShouldBe(9);

        }
    }
}