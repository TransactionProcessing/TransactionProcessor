namespace TransactionProcessor.FloatAggregate
{
    using System.Diagnostics.CodeAnalysis;
    using Float.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;

    public static class FloatAggregateExtensions
    {
        public static void PlayEvent(this FloatAggregate aggregate, FloatCreatedForContractProductEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.ContractId = domainEvent.ContractId;
            aggregate.ProductId = domainEvent.ProductId;
            aggregate.CreatedDateTime = domainEvent.CreatedDateTime;
            aggregate.IsCreated = true;
        }

        public static void PlayEvent(this FloatAggregate aggregate, FloatCreditPurchasedEvent domainEvent)
        {
            aggregate.Balance += domainEvent.Amount;
            aggregate.NumberOfCreditPurchases++;
            aggregate.TotalCreditPurchases += domainEvent.Amount;
            aggregate.TotalCostPrice += domainEvent.CostPrice;
            aggregate.UnitCostPrice = (aggregate.TotalCostPrice / aggregate.TotalCreditPurchases);
            aggregate.Credits.Add((domainEvent.CreditPurchasedDateTime, domainEvent.Amount,domainEvent.CostPrice));
        }

        public static void PlayEvent(this FloatAggregate aggregate, FloatDecreasedByTransactionEvent domainEvent){
            aggregate.Balance -= domainEvent.Amount;
            aggregate.NumberOfTransactions++;
            aggregate.TotalTransactions += domainEvent.Amount;
            aggregate.Transactions.Add((domainEvent.TransactionId, domainEvent.Amount));
        }

        public static void CreateFloat(this FloatAggregate aggregate, 
                                               Guid estateId,
                                               Guid contractId,
                                               Guid productId,
                                               DateTime createdDateTime)
        {
            aggregate.ValidateFloatIsNotAlreadyCreated();

            FloatCreatedForContractProductEvent floatCreatedForContractProductEvent = new FloatCreatedForContractProductEvent(aggregate.AggregateId,
                estateId, contractId, productId, createdDateTime);

            aggregate.ApplyAndAppend(floatCreatedForContractProductEvent);
        }

        public static void RecordCreditPurchase(this FloatAggregate aggregate, DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)
        {
            aggregate.ValidateFloatIsAlreadyCreated();
            aggregate.ValidateCreditIsNotADuplicate(creditPurchasedDate, amount, costPrice);
            
            FloatCreditPurchasedEvent floatCreditPurchasedEvent = new FloatCreditPurchasedEvent(aggregate.AggregateId, aggregate.EstateId,
                                                                                                creditPurchasedDate, amount, costPrice);

            aggregate.ApplyAndAppend(floatCreditPurchasedEvent);
        }

        public static void RecordTransactionAgainstFloat(this FloatAggregate aggregate, Guid transactionId, Decimal transactionAmount){
            aggregate.ValidateFloatIsAlreadyCreated();

            Boolean isTransactionADuplicate = aggregate.IsTransactionADuplicate(transactionId);
            if (isTransactionADuplicate)
                return;

            FloatDecreasedByTransactionEvent floatDecreasedByTransactionEvent = new FloatDecreasedByTransactionEvent(aggregate.AggregateId, aggregate.EstateId, transactionId, transactionAmount);

            aggregate.ApplyAndAppend(floatDecreasedByTransactionEvent);
        }

        public static Decimal GetUnitCostPrice(this FloatAggregate aggregate)
        {
            return Math.Round(aggregate.UnitCostPrice, 4);
        }

        public static void ValidateFloatIsAlreadyCreated(this FloatAggregate aggregate){
            if (aggregate.IsCreated == false){
                throw new InvalidOperationException($"Float Aggregate Id {aggregate.AggregateId} must be created to perform this operation");
            }
        }

        public static void ValidateFloatIsNotAlreadyCreated(this FloatAggregate aggregate){
            if (aggregate.IsCreated == true){
                throw new InvalidOperationException($"Float Aggregate Id {aggregate.AggregateId} must not be created to perform this operation");
            }
        }

        public static void ValidateCreditIsNotADuplicate(this FloatAggregate aggregate, DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)
        {
            Boolean isDuplicate = aggregate.Credits.Any(c => c.costPrice == costPrice && c.amount == amount && c.creditPurchasedDate == creditPurchasedDate);
            if (isDuplicate == true)
            {
                throw new InvalidOperationException($"Float Aggregate Id {aggregate.AggregateId} already has a credit with this information recorded");
            }
        }

        public static Boolean IsTransactionADuplicate(this FloatAggregate aggregate, Guid transactionId)
        {
            Boolean isDuplicate = aggregate.Transactions.Any(c => c.transactionId == transactionId);
            return isDuplicate;
        }
    }

    public record FloatAggregate : Aggregate
    {
        public override void PlayEvent(IDomainEvent domainEvent) => FloatAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
                   {
                       this.EstateId
                   };
        }


        [ExcludeFromCodeCoverage]
        public FloatAggregate(){
            this.Transactions = new List<(Guid transactionId, Decimal amount)>();
            this.Credits = new List<(DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)>();
        }

        private FloatAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Credits = new List<(DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)>();
            this.Transactions = new List<(Guid transactionId, Decimal amount)>();
        }

        internal List<(DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)> Credits;
        internal List<(Guid transactionId, Decimal amount)> Transactions;

        public Boolean IsCreated { get; internal set; }

        public Guid EstateId { get; internal set; }

        public Guid ContractId { get; internal set; }
        public Guid ProductId { get; internal set; }
        public DateTime CreatedDateTime { get; internal set; }

        public Int32 NumberOfCreditPurchases { get; internal set; }
        public Int32 NumberOfTransactions { get; internal set; }
        public Decimal Balance { get; internal set; }
        public Decimal TotalCreditPurchases { get; internal set; }
        public Decimal TotalTransactions { get; internal set; }

        public Decimal TotalCostPrice { get; internal set; }
        public Decimal UnitCostPrice { get; internal set; }

        public static FloatAggregate Create(Guid aggregateId)
        {
            return new FloatAggregate(aggregateId);
        }
    }
}
