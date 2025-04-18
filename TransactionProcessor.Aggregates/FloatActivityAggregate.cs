using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.Aggregates
{
    public static class FloatActivityAggregateExtensions
    {
        public static void PlayEvent(this FloatActivityAggregate aggregate,
                                     FloatActivityDomainEvents.FloatAggregateCreditedEvent domainEvent)
        {
            aggregate.CreditCount++;
            aggregate.Credits.Add(domainEvent.CreditId);
        }

        public static void PlayEvent(this FloatActivityAggregate aggregate,
                                     FloatActivityDomainEvents.FloatAggregateDebitedEvent domainEvent)
        {
            aggregate.DebitCount++;
            aggregate.Debits.Add(domainEvent.DebitId);
        }

        public static void RecordCreditPurchase(this FloatActivityAggregate aggregate,
                                                Guid estateId,
                                                DateTime activityDateTime,
                                                Decimal creditAmount,
                                                Guid creditId)
        {

            if (aggregate.Credits.Any(c => c == creditId))
                return;

            FloatActivityDomainEvents.FloatAggregateCreditedEvent floatAggregateCreditedEvent = new(aggregate.AggregateId, estateId, activityDateTime, creditAmount, creditId);
            aggregate.ApplyAndAppend(floatAggregateCreditedEvent);
        }

        public static void RecordTransactionAgainstFloat(this FloatActivityAggregate aggregate,
                                                         Guid estateId,
                                                         DateTime activityDateTime,
                                                         Decimal transactionAmount,
                                                         Guid transactionId)
        {
            if (aggregate.Debits.Any(c => c == transactionId))
                return;

            FloatActivityDomainEvents.FloatAggregateDebitedEvent floatAggregateCreditedEvent = new(aggregate.AggregateId, estateId, activityDateTime, transactionAmount, transactionId);
            aggregate.ApplyAndAppend(floatAggregateCreditedEvent);
        }
    }

    public record FloatActivityAggregate : Aggregate
    {
        public override void PlayEvent(IDomainEvent domainEvent) => FloatActivityAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        public Int32 CreditCount { get; internal set; }
        public Int32 DebitCount { get; internal set; }
        public List<Guid> Credits { get; internal set; }
        public List<Guid> Debits { get; internal set; }
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
            {

            };
        }


        [ExcludeFromCodeCoverage]
        public FloatActivityAggregate()
        {
            this.Credits = new List<Guid>();
            this.Debits = new List<Guid>();
        }

        private FloatActivityAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Credits = new List<Guid>();
            this.Debits = new List<Guid>();
        }

        public static FloatActivityAggregate Create(Guid aggregateId)
        {
            return new FloatActivityAggregate(aggregateId);
        }
    }
}
