using Shouldly;
using SimpleResults;
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
            var result = aggregate.Create(TestData.EstateId, TestData.OperatorName, TestData.RequireCustomMerchantNumber, TestData.RequireCustomTerminalNumber);
            result.IsSuccess.ShouldBeTrue();

            aggregate.AggregateId.ShouldBe(TestData.OperatorId);
            aggregate.Name.ShouldBe(TestData.OperatorName);
            aggregate.IsCreated.ShouldBeTrue();
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.RequireCustomTerminalNumber.ShouldBe(TestData.RequireCustomTerminalNumber);
            aggregate.RequireCustomMerchantNumber.ShouldBe(TestData.RequireCustomMerchantNumber);
        }

        [Fact]
        public void OperatorAggregate_Create_EstateIdEmpty_ErrorReturned()
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);
            var result = aggregate.Create(Guid.Empty, TestData.OperatorName, TestData.RequireCustomMerchantNumber, TestData.RequireCustomTerminalNumber);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("Estate Id must not be an empty Guid");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void OperatorAggregate_Create_NameInvalid_ErrorReturned(String operatorName)
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);
            var result = aggregate.Create(TestData.EstateId, operatorName, TestData.RequireCustomMerchantNumber, TestData.RequireCustomTerminalNumber);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldBe("Operator name must not be null or empty");
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

        [Theory]
        [InlineData("Alice", "Bob", "Bob")]
        [InlineData("alice", "Alice", "alice")]
        [InlineData("Alice", null, "Alice")]
        [InlineData("Alice", "", "Alice")]
        public void OperatorAggregate_UpdateOperator_OperatorName_IsUpdated(String existingName, String newName, String expectedName)
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);
            aggregate.Create(TestData.EstateId, existingName, TestData.RequireCustomMerchantNumberTrue, TestData.RequireCustomTerminalNumberTrue);

            Result result = aggregate.UpdateOperator(newName, TestData.RequireCustomMerchantNumberTrue, TestData.RequireCustomTerminalNumberTrue);
            result.IsSuccess.ShouldBeTrue();
            
            aggregate.Name.ShouldBe(expectedName);
        }

        [Fact]
        public void OperatorAggregate_UpdateOperator_RequireCustomMerchantNumber_IsUpdated()
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);
            aggregate.Create(TestData.EstateId, TestData.OperatorName, TestData.RequireCustomMerchantNumberTrue, TestData.RequireCustomTerminalNumberTrue);

            Result result = aggregate.UpdateOperator(TestData.OperatorName, TestData.RequireCustomMerchantNumberFalse, TestData.RequireCustomTerminalNumberTrue);
            result.IsSuccess.ShouldBeTrue();

            aggregate.RequireCustomMerchantNumber.ShouldBe(TestData.RequireCustomMerchantNumberFalse);
        }

        [Fact]
        public void OperatorAggregate_UpdateOperator_RequireCustomTerminalNumber_IsUpdated()
        {
            OperatorAggregate aggregate = OperatorAggregate.Create(TestData.OperatorId);
            aggregate.Create(TestData.EstateId, TestData.OperatorName, TestData.RequireCustomMerchantNumberTrue, TestData.RequireCustomTerminalNumberTrue);

            Result result = aggregate.UpdateOperator(TestData.OperatorName, TestData.RequireCustomMerchantNumberTrue, TestData.RequireCustomTerminalNumberFalse);
            result.IsSuccess.ShouldBeTrue();

            aggregate.RequireCustomTerminalNumber.ShouldBe(TestData.RequireCustomTerminalNumberFalse);
        }
    }
}
