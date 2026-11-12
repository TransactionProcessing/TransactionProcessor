using System.Diagnostics.CodeAnalysis;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

[ExcludeFromCodeCoverage]
public class FloatActivityDomainEvents {
    public record FloatAggregateCreditedEvent(Guid FloatId, Guid EstateId, DateTimeOffset ActivityDateTime, Decimal Amount, Guid CreditId) : DomainEvent(FloatId, Guid.NewGuid());
    public record FloatAggregateDebitedEvent(Guid FloatId, Guid EstateId, DateTimeOffset ActivityDateTime, Decimal Amount, Guid DebitId) : DomainEvent(FloatId, Guid.NewGuid());
}