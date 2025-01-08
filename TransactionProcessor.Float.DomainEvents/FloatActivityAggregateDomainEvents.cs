using Shared.DomainDrivenDesign.EventSourcing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.Float.DomainEvents
{
    [ExcludeFromCodeCoverage]
    public record FloatAggregateCreditedEvent(Guid FloatId,
                       Guid EstateId,
                       DateTime ActivityDateTime,
                       Decimal Amount, Guid CreditId) : DomainEvent(FloatId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record FloatAggregateDebitedEvent(Guid FloatId,
                                              Guid EstateId,
                                              DateTime ActivityDateTime,
                                              Decimal Amount, Guid DebitId) : DomainEvent(FloatId, Guid.NewGuid());
}
