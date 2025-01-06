using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.General;
using TransactionProcessor.Float.DomainEvents;

namespace TransactionProcessor.FloatAggregate;

public static class FloatActivityAggregateExtensions
{
    public static void PlayEvent(this FloatActivityAggregate aggregate, FloatAggregateCreditedEvent domainEvent)
    {
    }

    public static void PlayEvent(this FloatActivityAggregate aggregate, FloatAggregateDebitedEvent domainEvent)
    {
    }

    public static void RecordCreditPurchase(this FloatActivityAggregate aggregate,
                                            Guid estateId,
                                            DateTime activityDateTime,
                                            Decimal creditAmount) {
        FloatAggregateCreditedEvent floatAggregateCreditedEvent =
            new (aggregate.AggregateId, estateId, activityDateTime, creditAmount);
        aggregate.ApplyAndAppend(floatAggregateCreditedEvent);
    }

    public static void RecordTransactionAgainstFloat(this FloatActivityAggregate aggregate, Guid estateId, DateTime activityDateTime, Decimal transactionAmount)
    {
        FloatAggregateDebitedEvent floatAggregateCreditedEvent =
            new (aggregate.AggregateId, estateId, activityDateTime, transactionAmount);
        aggregate.ApplyAndAppend(floatAggregateCreditedEvent);
    }
}

public record FloatActivityAggregate : Aggregate {
    public override void PlayEvent(IDomainEvent domainEvent) => FloatActivityAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

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
        
    }

    private FloatActivityAggregate(Guid aggregateId)
    {
        Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

        this.AggregateId = aggregateId;
    }

    public static FloatActivityAggregate Create(Guid aggregateId)
    {
        return new FloatActivityAggregate(aggregateId);
    }
}