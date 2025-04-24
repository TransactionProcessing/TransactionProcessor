using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

[ExcludeFromCodeCoverage]
public class FloatActivityDomainEvents {
    public record FloatAggregateCreditedEvent(Guid FloatId, Guid EstateId, DateTime ActivityDateTime, Decimal Amount, Guid CreditId) : DomainEvent(FloatId, Guid.NewGuid());
    public record FloatAggregateDebitedEvent(Guid FloatId, Guid EstateId, DateTime ActivityDateTime, Decimal Amount, Guid DebitId) : DomainEvent(FloatId, Guid.NewGuid());
}