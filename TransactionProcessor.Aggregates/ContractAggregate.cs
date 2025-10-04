using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using SimpleResults;
using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models.Contract;
using static Google.Protobuf.Reflection.FeatureSet.Types;

namespace TransactionProcessor.Aggregates{
    public static class ContractAggregateExtensions{
        #region Methods

        public static Result AddFixedValueProduct(this ContractAggregate aggregate,
                                                Guid productId,
                                                String productName,
                                                String displayText,
                                                Decimal value,
                                                ProductType productType){
            if (productId == Guid.Empty)
                return Result.Invalid("Product Id must not be an empty Guid");
            if (String.IsNullOrEmpty(productName))
                return Result.Invalid("Product Name must not be null or empty");
            if (String.IsNullOrEmpty(displayText))
                return Result.Invalid("Product Display Text must not be null or empty");
            if (value <= 0)
                return Result.Invalid("Product value must not be zero or negative");
            
            // Check product not already added
            if (aggregate.Products.Any(p => p.Name == productName)){
                return Result.Success();
            }

            ContractDomainEvents.FixedValueProductAddedToContractEvent fixedValueProductAddedToContractEvent =
                new(aggregate.AggregateId, aggregate.EstateId, productId, productName, displayText, value, (Int32)productType);

            aggregate.ApplyAndAppend(fixedValueProductAddedToContractEvent);

            return Result.Success();
        }

        public static Result AddTransactionFee(this ContractAggregate aggregate,
                                             Product product,
                                             Guid transactionFeeId,
                                             String description,
                                             CalculationType calculationType,
                                             FeeType feeType,
                                             Decimal value){
            if (transactionFeeId == Guid.Empty)
                return Result.Invalid("Transaction Fee Id cannot be an empty Guid");
            if (product == null)
                return Result.Invalid("Product to add fee for cannot be null");
            if (String.IsNullOrEmpty(description))
                return Result.Invalid("Transaction Fee description must not be null or empty");
            if (value <= 0)
                return Result.Invalid("Transaction Fee value must not be zero or negative");
            
            if (aggregate.Products.Any(p => p.ContractProductId == product.ContractProductId) == false){
                return Result.Invalid($"Product Id {product.ContractProductId} is not a valid product on this contract");
            }
            if (Enum.IsDefined(typeof(CalculationType), calculationType) == false)
                return Result.Invalid("Calculation Type not valid");
            if (Enum.IsDefined(typeof(FeeType), feeType) == false)
                return Result.Invalid("Fee Type not valid");
            
            ContractDomainEvents.TransactionFeeForProductAddedToContractEvent transactionFeeForProductAddedToContractEvent =
                new(aggregate.AggregateId, aggregate.EstateId, product.ContractProductId, transactionFeeId, description, (Int32)calculationType, (Int32)feeType, value);

            aggregate.ApplyAndAppend(transactionFeeForProductAddedToContractEvent);

            return Result.Success();
        }

        public static Result AddVariableValueProduct(this ContractAggregate aggregate,
                                                   Guid productId,
                                                   String productName,
                                                   String displayText,
                                                   ProductType productType){
            if (productId == Guid.Empty)
                return Result.Invalid("Product Id must not be an empty Guid");
            if (String.IsNullOrEmpty(productName))
                return Result.Invalid("Product Name must not be null or empty");
            if (String.IsNullOrEmpty(displayText))
                return Result.Invalid("Product Display Text must not be null or empty");

            // Check product not already added
            if (aggregate.Products.Any(p => p.Name == productName)){
                return Result.Success();
            }

            ContractDomainEvents.VariableValueProductAddedToContractEvent variableValueProductAddedToContractEvent =
                new(aggregate.AggregateId, aggregate.EstateId, productId, productName, displayText, (Int32)productType);

            aggregate.ApplyAndAppend(variableValueProductAddedToContractEvent);
            
            return Result.Success();
        }

        public static Result Create(this ContractAggregate aggregate,
                                  Guid estateId,
                                  Guid operatorId,
                                  String description){
            
            if(estateId == Guid.Empty) 
                return Result.Invalid("Estate Id must not be an empty Guid");
            if (operatorId == Guid.Empty)
                return Result.Invalid("Operator Id must not be an empty Guid");
            if (String.IsNullOrEmpty(description))
                return Result.Invalid("Contract description must not be null or empty");

            ContractDomainEvents.ContractCreatedEvent contractCreatedEvent = new(aggregate.AggregateId, estateId, operatorId, description);
            aggregate.ApplyAndAppend(contractCreatedEvent);

            return Result.Success();
        }

        public static Result DisableTransactionFee(this ContractAggregate aggregate,
                                                 Guid productId,
                                                 Guid transactionFeeId){
            if (aggregate.Products.Any(p => p.ContractProductId == productId) == false){
                return Result.Invalid($"Product Id {productId} is not a valid product on this contract");
            }

            Product product = aggregate.Products.Single(p => p.ContractProductId == productId);

            if (product.TransactionFees.Any(f => f.TransactionFeeId == transactionFeeId) == false){
                return Result.Invalid($"Transaction Fee Id {transactionFeeId} is not a valid for product {product.Name} on this contract");
            }

            ContractDomainEvents.TransactionFeeForProductDisabledEvent transactionFeeForProductDisabledEvent = new(aggregate.AggregateId,
                                                                                                                   aggregate.EstateId,
                                                                                                                   productId,
                                                                                                                   transactionFeeId);

            aggregate.ApplyAndAppend(transactionFeeForProductDisabledEvent);

            return Result.Success();
        }

        /// <summary>
        /// Gets the contract.
        /// </summary>
        /// <returns></returns>
        public static Contract GetContract(this ContractAggregate aggregate){
            Contract contractModel = new Contract();

            contractModel.EstateId = aggregate.EstateId;
            contractModel.IsCreated = aggregate.IsCreated;
            contractModel.OperatorId = aggregate.OperatorId;
            contractModel.Description = aggregate.Description;
            contractModel.ContractId = aggregate.AggregateId;
            contractModel.Products = aggregate.Products;

            return contractModel;
        }

        public static List<Product> GetProducts(this ContractAggregate aggregate){
            return aggregate.Products;
        }

        public static void PlayEvent(this ContractAggregate aggregate, ContractDomainEvents.TransactionFeeForProductDisabledEvent domainEvent){
            // Find the product
            Product product = aggregate.Products.Single(p => p.ContractProductId == domainEvent.ProductId);
            ContractProductTransactionFee transactionFee = product.TransactionFees.Single(t => t.TransactionFeeId == domainEvent.TransactionFeeId);

            transactionFee.IsEnabled = false;
        }

        public static void PlayEvent(this ContractAggregate aggregate, ContractDomainEvents.ContractCreatedEvent domainEvent){
            aggregate.IsCreated = true;
            aggregate.OperatorId = domainEvent.OperatorId;
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.Description = domainEvent.Description;
        }

        public static void PlayEvent(this ContractAggregate aggregate, ContractDomainEvents.FixedValueProductAddedToContractEvent domainEvent){
            aggregate.Products.Add(new Product{
                                                  ContractProductId = domainEvent.ProductId,
                                                  Name = domainEvent.ProductName,
                                                  DisplayText = domainEvent.DisplayText,
                                                  Value = domainEvent.Value,
                                                  ProductType = (ProductType)domainEvent.ProductType
                                              });
        }

        public static void PlayEvent(this ContractAggregate aggregate, ContractDomainEvents.VariableValueProductAddedToContractEvent domainEvent){
            aggregate.Products.Add(new Product{
                                                  ContractProductId = domainEvent.ProductId,
                                                  Name = domainEvent.ProductName,
                                                  DisplayText = domainEvent.DisplayText,
                                                  ProductType = (ProductType)domainEvent.ProductType
                                              });
        }

        public static void PlayEvent(this ContractAggregate aggregate, ContractDomainEvents.TransactionFeeForProductAddedToContractEvent domainEvent){
            // Find the product
            Product product = aggregate.Products.Single(p => p.ContractProductId == domainEvent.ProductId);

            product.TransactionFees.Add(new ContractProductTransactionFee(){
                                                              Description = domainEvent.Description,
                                                              CalculationType = (CalculationType)domainEvent.CalculationType,
                                                              TransactionFeeId = domainEvent.TransactionFeeId,
                                                              Value = domainEvent.Value,
                                                              IsEnabled = true,
                                                              FeeType = (FeeType)domainEvent.FeeType
                                                          });
        }

        #endregion
    }

    public record ContractAggregate : Aggregate{
        #region Fields

        internal readonly List<Product> Products;

        #endregion

        #region Constructors

        [ExcludeFromCodeCoverage]
        public ContractAggregate(){
            // Nothing here
            this.Products = new List<Product>();
        }

        private ContractAggregate(Guid aggregateId){
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Products = new List<Product>();
        }

        #endregion

        #region Properties

        public String Description{ get; internal set; }

        public Guid EstateId{ get; internal set; }

        public Boolean IsCreated{ get; internal set; }

        public Guid OperatorId{ get; internal set; }

        #endregion

        #region Methods

        public static ContractAggregate Create(Guid aggregateId){
            return new ContractAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => ContractAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata(){
            return new{
                          EstateId = Guid.NewGuid() 
                      };
        }

        #endregion
    }
}