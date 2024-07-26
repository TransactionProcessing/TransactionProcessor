using Shared.DomainDrivenDesign.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.Float.DomainEvents
{
    public record FloatAggregateCreditedEvent(Guid FloatId,
                       Guid EstateId,
                       DateTime ActivityDateTime,
                       Decimal Amount) : DomainEvent(FloatId, Guid.NewGuid());

    public record FloatAggregateDebitedEvent(Guid FloatId,
                                              Guid EstateId,
                                              DateTime ActivityDateTime,
                                              Decimal Amount) : DomainEvent(FloatId, Guid.NewGuid());
}
