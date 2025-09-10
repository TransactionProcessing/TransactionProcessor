using Shouldly;
using SimpleResults;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests
{
    public class ContractAggregateTests
    {
        [Fact]
        public void ContractAggregate_CanBeCreated_IsCreated()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);

            aggregate.AggregateId.ShouldBe(TestData.ContractId);
        }

        [Fact]
        public void ContractAggregate_Create_IsCreated()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            Result result = aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            result.IsSuccess.ShouldBeTrue();

            aggregate.AggregateId.ShouldBe(TestData.ContractId);
            aggregate.EstateId.ShouldBe(TestData.EstateId);
            aggregate.OperatorId.ShouldBe(TestData.OperatorId);
            aggregate.Description.ShouldBe(TestData.ContractDescription);
            aggregate.IsCreated.ShouldBeTrue();
        }

        [Fact]
        public void ContractAggregate_Create_InvalidEstateId_ErrorThrown()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);

            Result result = aggregate.Create(Guid.Empty, TestData.OperatorId, TestData.ContractDescription);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Estate Id must not be an empty Guid");
        }

        [Fact]
        public void ContractAggregate_Create_InvalidOperatorId_ErrorThrown()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);

            Result result = aggregate.Create(TestData.EstateId, Guid.Empty, TestData.ContractDescription);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Operator Id must not be an empty Guid");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ContractAggregate_Create_InvalidDescription_ErrorThrown(String description)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);

            Result result = aggregate.Create(TestData.EstateId, TestData.OperatorId, description);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Contract description must not be null or empty");
        }

        [Fact]
        public void ContractAggregate_AddFixedValueProduct_ProductAdded()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result = aggregate.AddFixedValueProduct(TestData.FixedContractProductId,
                                           TestData.ProductName,
                                           TestData.ProductDisplayText,
                                           TestData.ProductFixedValue,
                                           TestData.ProductTypeMobileTopup);
            result.IsSuccess.ShouldBeTrue();

            List<Product> products = aggregate.GetProducts();
            products.Count.ShouldBe(1);
            Product product= products.Single();
            product.ContractProductId.ShouldNotBe(Guid.Empty);
            product.Name.ShouldBe(TestData.ProductName);
            product.DisplayText.ShouldBe(TestData.ProductDisplayText);
            product.Value.ShouldBe(TestData.ProductFixedValue);
            product.Value.ShouldBe(TestData.ProductFixedValue);
            product.ProductType.ShouldBe(TestData.ProductTypeMobileTopup);
        }

        [Fact]
        public void ContractAggregate_AddFixedValueProduct_DuplicateProduct_NoErrorThrown()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            aggregate.AddFixedValueProduct(TestData.FixedContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductFixedValue, TestData.ProductTypeMobileTopup);
            List<Product> products = aggregate.GetProducts();
            products.Count.ShouldBe(1);

            Result result = aggregate.AddFixedValueProduct(TestData.FixedContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductFixedValue, TestData.ProductTypeMobileTopup);
            result.IsSuccess.ShouldBeTrue();
            products = aggregate.GetProducts();
            products.Count.ShouldBe(1);
        }

        [Fact]
        public void ContractAggregate_AddFixedValueProduct_InvalidProductId_ErrorThrown()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result = aggregate.AddFixedValueProduct(Guid.Empty, 
                TestData.ProductName,
                TestData.ProductDisplayText,
                TestData.ProductFixedValue,
                TestData.ProductTypeMobileTopup);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Product Id must not be an empty Guid");
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ContractAggregate_AddFixedValueProduct_InvalidProductName_ErrorThrown(String productName)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            
            Result result = aggregate.AddFixedValueProduct(TestData.FixedContractProductId, productName, TestData.ProductDisplayText, TestData.ProductFixedValue, TestData.ProductTypeMobileTopup);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Product Name must not be null or empty");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ContractAggregate_AddFixedValueProduct_InvalidProductDisplayText_ErrorThrown(String displayText)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result = aggregate.AddFixedValueProduct(TestData.FixedContractProductId, TestData.ProductName, displayText, TestData.ProductFixedValue,TestData.ProductTypeMobileTopup);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Product Display Text must not be null or empty");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ContractAggregate_AddFixedValueProduct_InvalidProductValue_ErrorThrown(Decimal value)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result =  aggregate.AddFixedValueProduct(TestData.FixedContractProductId, TestData.ProductName, TestData.ProductDisplayText, value, TestData.ProductTypeMobileTopup);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Product value must not be zero or negative");
        }

        [Fact]
        public void ContractAggregate_AddVariableValueProduct_ProductAdded()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result = aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);
            result.IsSuccess.ShouldBeTrue();

            List<Product> products = aggregate.GetProducts();
            products.Count.ShouldBe(1);
            Product product = products.Single();
            product.ContractProductId.ShouldNotBe(Guid.Empty);
            product.Name.ShouldBe(TestData.ProductName);
            product.DisplayText.ShouldBe(TestData.ProductDisplayText);
            product.Value.ShouldBeNull();
            product.ProductType.ShouldBe(TestData.ProductTypeMobileTopup);

        }

        [Fact]
        public void ContractAggregate_AddVariableValueProduct_DuplicateProduct_ErrorThrown()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);
            List<Product> products = aggregate.GetProducts();
            products.Count.ShouldBe(1);
            
            Result result = aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);
            result.IsSuccess.ShouldBeTrue();
            products = aggregate.GetProducts();
            products.Count.ShouldBe(1);
        }

        [Fact]
        public void ContractAggregate_AddVariableValueProduct_InvalidProductId_ErrorThrown()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result = aggregate.AddVariableValueProduct(Guid.Empty,
                TestData.ProductName,
                TestData.ProductDisplayText,
                TestData.ProductTypeMobileTopup);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Product Id must not be an empty Guid");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ContractAggregate_AddVariableValueProduct_InvalidProductName_ErrorThrown(String productName)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result = aggregate.AddVariableValueProduct(TestData.VariableContractProductId, productName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Product Name must not be null or empty");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ContractAggregate_AddVariableValueProduct_InvalidProductDisplayText_ErrorThrown(String displayText)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result = aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, displayText, TestData.ProductTypeMobileTopup);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
            result.Message.ShouldContain("Product Display Text must not be null or empty");
        }

        [Fact]
        public void ContractAggregate_GetContract_ContractReturned()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Contract contract = aggregate.GetContract();

            contract.ContractId.ShouldBe(TestData.ContractId);
            contract.Description.ShouldBe(TestData.ContractDescription);
            contract.IsCreated.ShouldBeTrue();
            contract.OperatorId.ShouldBe(TestData.OperatorId);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider)]
        public void ContractAggregate_AddTransactionFee_FixedValueProduct_TransactionFeeAdded(CalculationType calculationType, FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddFixedValueProduct(TestData.FixedContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductFixedValue, TestData.ProductTypeMobileTopup);
            
            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            Result result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType, feeType, TestData.TransactionFeeValue);
            result.IsSuccess.ShouldBeTrue();

            List<Product> productsAfterFeeAdded = aggregate.GetProducts();
            Product productWithFees = productsAfterFeeAdded.Single();

            productWithFees.TransactionFees.ShouldHaveSingleItem();
            ContractProductTransactionFee? fee = productWithFees.TransactionFees.Single();
            fee.Description.ShouldBe(TestData.TransactionFeeDescription);
            fee.TransactionFeeId.ShouldBe(TestData.TransactionFeeId);
            fee.CalculationType.ShouldBe(calculationType);
            fee.FeeType.ShouldBe(feeType);
            fee.Value.ShouldBe(TestData.TransactionFeeValue);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider)]
        public void ContractAggregate_AddTransactionFee_FixedValueProduct_InvalidFeeId_ErrorThrown(CalculationType calculationType, FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddFixedValueProduct(TestData.FixedContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductFixedValue, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, Guid.Empty, TestData.TransactionFeeDescription, calculationType, feeType, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Fixed,FeeType.Merchant, null)]
        [InlineData(CalculationType.Fixed, FeeType.Merchant, "")]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider, null)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider, "")]
        [InlineData(CalculationType.Percentage, FeeType.Merchant, null)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant, "")]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider, null)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider, "")]
        public void ContractAggregate_AddTransactionFee_FixedValueProduct_InvalidFeeDescription_ErrorThrown(CalculationType calculationType, FeeType feeType, String feeDescription)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddFixedValueProduct(TestData.FixedContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductFixedValue, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, feeDescription, calculationType, feeType, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider)]
        public void ContractAggregate_AddTransactionFee_VariableValueProduct_TransactionFeeAdded(CalculationType calculationType, FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType, feeType, TestData.TransactionFeeValue);
            result.IsSuccess.ShouldBeTrue();

            List<Product> productsAfterFeeAdded = aggregate.GetProducts();
            Product productWithFees = productsAfterFeeAdded.Single();

            productWithFees.TransactionFees.ShouldHaveSingleItem();
            ContractProductTransactionFee? fee = productWithFees.TransactionFees.Single();
            fee.Description.ShouldBe(TestData.TransactionFeeDescription);
            fee.TransactionFeeId.ShouldBe(TestData.TransactionFeeId);
            fee.CalculationType.ShouldBe(calculationType);
            fee.FeeType.ShouldBe(feeType);
            fee.Value.ShouldBe(TestData.TransactionFeeValue);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider)]
        public void ContractAggregate_AddTransactionFee_VariableValueProduct_InvalidFeeId_ErrorThrown(CalculationType calculationType, FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, Guid.Empty, TestData.TransactionFeeDescription, calculationType, feeType, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant, null)]
        [InlineData(CalculationType.Fixed, FeeType.Merchant, "")]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider, null)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider, "")]
        [InlineData(CalculationType.Percentage, FeeType.Merchant, null)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant, "")]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider, null)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider, "")]
        public void ContractAggregate_AddTransactionFee_VariableValueProduct_InvalidFeeDescription_ErrorThrown(CalculationType calculationType, FeeType feeType, String feeDescription)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, feeDescription, calculationType, feeType, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider)]
        public void ContractAggregate_AddTransactionFee_NullProduct_ErrorThrown(CalculationType calculationType, FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            var result = aggregate.AddTransactionFee(null, TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType, feeType, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider)]
        public void ContractAggregate_AddTransactionFee_ProductNotFound_ErrorThrown(CalculationType calculationType, FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddFixedValueProduct(TestData.FixedContractProductId,TestData.ProductName, TestData.ProductDisplayText, TestData.ProductFixedValue, TestData.ProductTypeMobileTopup);

            var result =  aggregate.AddTransactionFee(new Product(), TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType, feeType,TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(FeeType.Merchant)]
        [InlineData(FeeType.ServiceProvider)]
        public void ContractAggregate_AddTransactionFee_FixedValueProduct_InvalidCalculationType_ErrorThrown(FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, (CalculationType)99, feeType, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Percentage)]
        [InlineData(CalculationType.Fixed)]
        public void ContractAggregate_AddTransactionFee_FixedValueProduct_InvalidFeeType_ErrorThrown(CalculationType calculationType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType, (FeeType)99, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant,0)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant, 0)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider, 0)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider, 0)]
        [InlineData(CalculationType.Fixed, FeeType.Merchant, -1)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant, -1)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider, -1)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider, -1)]
        public void ContractAggregate_AddTransactionFee_VariableValueProduct_InvalidFeeValue_ErrorThrown(CalculationType calculationType, FeeType feeType, Decimal feeValue)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType, feeType, feeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant, 0)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant, 0)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider, 0)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider, 0)]
        [InlineData(CalculationType.Fixed, FeeType.Merchant, -1)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant, -1)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider, -1)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider, -1)]
        public void ContractAggregate_AddTransactionFee_FixedValueProduct_InvalidFeeValue_ErrorThrown(CalculationType calculationType, FeeType feeType, Decimal feeValue)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result =  aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType,feeType, feeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);

        }

        [Theory]
        [InlineData(FeeType.Merchant)]
        [InlineData(FeeType.ServiceProvider)]
        public void ContractAggregate_AddTransactionFee_VariableValueProduct_InvalidCalculationType_ErrorThrown(FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, (CalculationType)99, feeType, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Percentage)]
        [InlineData(CalculationType.Fixed)]
        public void ContractAggregate_AddTransactionFee_VariableValueProduct_InvalidFeeType_ErrorThrown(CalculationType calculationType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            var result = aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType,(FeeType)99, TestData.TransactionFeeValue);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(CalculationType.Fixed, FeeType.Merchant)]
        [InlineData(CalculationType.Percentage, FeeType.Merchant)]
        [InlineData(CalculationType.Fixed, FeeType.ServiceProvider)]
        [InlineData(CalculationType.Percentage, FeeType.ServiceProvider)]
        public void ContractAggregate_DisableTransactionFee_TransactionFeeIsDisabled(CalculationType calculationType, FeeType feeType)
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);

            List<Product> products = aggregate.GetProducts();
            Product product = products.Single();

            aggregate.AddTransactionFee(product, TestData.TransactionFeeId, TestData.TransactionFeeDescription, calculationType, feeType, TestData.TransactionFeeValue);

            List<Product> productsAfterFeeAdded = aggregate.GetProducts();
            Product productWithFees = productsAfterFeeAdded.Single();
            productWithFees.TransactionFees.ShouldHaveSingleItem();
            ContractProductTransactionFee? fee = productWithFees.TransactionFees.Single();
            fee.IsEnabled.ShouldBeTrue();

            Result result = aggregate.DisableTransactionFee(TestData.VariableContractProductId, TestData.TransactionFeeId);
            result.IsSuccess.ShouldBeTrue();

            productsAfterFeeAdded = aggregate.GetProducts();
            productWithFees = productsAfterFeeAdded.Single();
            productWithFees.TransactionFees.ShouldHaveSingleItem();
            fee = productWithFees.TransactionFees.Single();
            fee.IsEnabled.ShouldBeFalse();
        }

        [Fact]
        public void ContractAggregate_DisableTransactionFee_ProductNotFound_ErrorThrown() {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);

            Result result = aggregate.DisableTransactionFee(TestData.VariableContractProductId, TestData.TransactionFeeId);

            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void ContractAggregate_DisableTransactionFee_TransactionFeeNotFound_ErrorThrown()
        {
            ContractAggregate aggregate = ContractAggregate.Create(TestData.ContractId);
            aggregate.Create(TestData.EstateId, TestData.OperatorId, TestData.ContractDescription);
            aggregate.AddVariableValueProduct(TestData.VariableContractProductId, TestData.ProductName, TestData.ProductDisplayText, TestData.ProductTypeMobileTopup);
            
            Result result = aggregate.DisableTransactionFee(TestData.VariableContractProductId, TestData.TransactionFeeId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
    }
}
