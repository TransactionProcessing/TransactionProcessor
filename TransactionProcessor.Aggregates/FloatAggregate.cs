using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using SimpleResults;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.Aggregates
{
    public static class FloatAggregateExtensions
    {
        public static void PlayEvent(this FloatAggregate aggregate, FloatDomainEvents.FloatCreatedForContractProductEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.ContractId = domainEvent.ContractId;
            aggregate.ProductId = domainEvent.ProductId;
            aggregate.CreatedDateTime = domainEvent.CreatedDateTime;
            aggregate.IsCreated = true;
        }

        public static void PlayEvent(this FloatAggregate aggregate, FloatDomainEvents.FloatCreditPurchasedEvent domainEvent)
        {
            aggregate.NumberOfCreditPurchases++;
            aggregate.TotalCreditPurchases += domainEvent.Amount;
            aggregate.TotalCostPrice += domainEvent.CostPrice;
            aggregate.UnitCostPrice = (aggregate.TotalCostPrice / aggregate.TotalCreditPurchases);
            aggregate.Credits.Add((domainEvent.CreditPurchasedDateTime, domainEvent.Amount, domainEvent.CostPrice));
        }

        public static Result CreateFloat(this FloatAggregate aggregate,
                                               Guid estateId,
                                               Guid contractId,
                                               Guid productId,
                                               DateTime createdDateTime)
        {
            if (aggregate.IsCreated) {
                return Result.Success(); // Idempotent
            }

            FloatDomainEvents.FloatCreatedForContractProductEvent floatCreatedForContractProductEvent = new(aggregate.AggregateId,
                estateId, contractId, productId, createdDateTime);

            aggregate.ApplyAndAppend(floatCreatedForContractProductEvent);

            return Result.Success();
        }

        public static Result RecordCreditPurchase(this FloatAggregate aggregate, DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)
        {
            Result result = aggregate.ValidateFloatIsAlreadyCreated();
            if (result.IsFailed) {
                return result;
            }
            result = aggregate.ValidateCreditIsNotADuplicate(creditPurchasedDate, amount, costPrice);
            if (result.IsFailed) {
                return result;
            }

            FloatDomainEvents.FloatCreditPurchasedEvent floatCreditPurchasedEvent = new(aggregate.AggregateId, aggregate.EstateId,
                                                                                                creditPurchasedDate, amount, costPrice);

            aggregate.ApplyAndAppend(floatCreditPurchasedEvent);

            return Result.Success();
        }

        public static Decimal GetUnitCostPrice(this FloatAggregate aggregate)
        {
            return Math.Round(aggregate.UnitCostPrice, 4);
        }

        public static Result ValidateFloatIsAlreadyCreated(this FloatAggregate aggregate) {
            if (aggregate.IsCreated == false) {
                return Result.Invalid($"Float Aggregate Id {aggregate.AggregateId} must be created to perform this operation");
            }

            return Result.Success();
        }

        public static Result ValidateCreditIsNotADuplicate(this FloatAggregate aggregate,
                                                           DateTime creditPurchasedDate,
                                                           Decimal amount,
                                                           Decimal costPrice) {
            Boolean isDuplicate = aggregate.Credits.Any(c => c.costPrice == costPrice && c.amount == amount && c.creditPurchasedDate == creditPurchasedDate);
            if (isDuplicate) {
                return Result.Invalid($"Float Aggregate Id {aggregate.AggregateId} already has a credit with this information recorded");
            }

            return Result.Success();
        }

        public static Decimal GetTotalCostPrice(this FloatAggregate aggregate, Decimal transactionAmount)
        {
            return transactionAmount * aggregate.UnitCostPrice;
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
        public FloatAggregate()
        {
            this.Credits = new List<(DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)>();
        }

        private FloatAggregate(Guid aggregateId)
        {
            if (aggregateId == Guid.Empty)
                throw new ArgumentNullException(nameof(aggregateId));

            this.AggregateId = aggregateId;
            this.Credits = new List<(DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)>();
        }

        internal List<(DateTime creditPurchasedDate, Decimal amount, Decimal costPrice)> Credits;

        public Boolean IsCreated { get; internal set; }

        public Guid EstateId { get; internal set; }

        public Guid ContractId { get; internal set; }
        public Guid ProductId { get; internal set; }
        public DateTime CreatedDateTime { get; internal set; }

        public Int32 NumberOfCreditPurchases { get; internal set; }


        public Decimal TotalCreditPurchases { get; internal set; }

        public Decimal TotalCostPrice { get; internal set; }
        public Decimal UnitCostPrice { get; internal set; }

        public static FloatAggregate Create(Guid aggregateId)
        {
            return new FloatAggregate(aggregateId);
        }
    }
}
