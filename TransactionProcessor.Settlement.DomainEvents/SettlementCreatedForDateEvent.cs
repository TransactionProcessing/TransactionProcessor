namespace TransactionProcessor.Settlement.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record SettlementCreatedForDateEvent(Guid SettlementId,
                                                Guid EstateId,
                                                Guid MerchantId,
                                                DateTime SettlementDate) : DomainEvent(SettlementId, Guid.NewGuid());
}