using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class OperatorAggregateTests
    {
        [Fact]
        public void OperatorAggregate_Create_OperatorIsCreated()
        {

        }

        [Fact]
        public void OperatorAggregate_CanBeCreated_IsCreated()
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);

            aggregate.AggregateId.ShouldBe(TestData.OperatorId);
        }

        [Fact]
        public void OperatorAggregate_Create_IsCreated()
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);
            aggregate.Create(TestData.EstateId, TestData.OperatorName, TestData.RequireCustomMerchantNumber, TestData.RequireCustomTerminalNumber);

            aggregate.AggregateId.ShouldBe(TestData.OperatorId);
            aggregate.Name.ShouldBe(TestData.OperatorName);
            aggregate.IsCreated.ShouldBeTrue();
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.RequireCustomTerminalNumber.ShouldBe(TestData.RequireCustomTerminalNumber);
            aggregate.RequireCustomMerchantNumber.ShouldBe(TestData.RequireCustomMerchantNumber);
        }

        [Fact]
        public void OperatorAggregate_GetOperator_OperatorIsReturned()
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);
            aggregate.Create(TestData.EstateId, TestData.OperatorName, TestData.RequireCustomMerchantNumber, TestData.RequireCustomTerminalNumber);
            TransactionProcessor.Models.Operator.Operator @operator = aggregate.GetOperator();
            @operator.OperatorId.ShouldBe(TestData.OperatorId);
            @operator.Name.ShouldBe(TestData.OperatorName);
            @operator.RequireCustomTerminalNumber.ShouldBe(TestData.RequireCustomTerminalNumber);
            @operator.RequireCustomMerchantNumber.ShouldBe(TestData.RequireCustomMerchantNumber);
        }

        [Fact]
        public void OperatorAggregate_UpdateOperator_IsUpdated()
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);
            aggregate.Create(TestData.EstateId, TestData.OperatorName, TestData.RequireCustomMerchantNumberFalse, TestData.RequireCustomTerminalNumberFalse);

            aggregate.Name.ShouldBe(TestData.OperatorName);
            aggregate.RequireCustomTerminalNumber.ShouldBe(TestData.RequireCustomMerchantNumberFalse);
            aggregate.RequireCustomMerchantNumber.ShouldBe(TestData.RequireCustomTerminalNumberFalse);

            aggregate.UpdateOperator(TestData.OperatorName2, TestData.RequireCustomMerchantNumberTrue, TestData.RequireCustomTerminalNumberTrue);

            aggregate.Name.ShouldBe(TestData.OperatorName2);
            aggregate.RequireCustomTerminalNumber.ShouldBe(TestData.RequireCustomMerchantNumberTrue);
            aggregate.RequireCustomMerchantNumber.ShouldBe(TestData.RequireCustomTerminalNumberTrue);
        }
    }
}
