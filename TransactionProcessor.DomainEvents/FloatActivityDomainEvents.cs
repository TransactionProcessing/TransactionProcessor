using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

public class FloatActivityDomainEvents {
    [ExcludeFromCodeCoverage]
    public record FloatAggregateCreditedEvent(Guid FloatId, Guid EstateId, DateTime ActivityDateTime, Decimal Amount, Guid CreditId) : DomainEvent(FloatId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record FloatAggregateDebitedEvent(Guid FloatId, Guid EstateId, DateTime ActivityDateTime, Decimal Amount, Guid DebitId) : DomainEvent(FloatId, Guid.NewGuid());
}