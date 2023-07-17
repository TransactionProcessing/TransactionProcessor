namespace TransactionProcessor.Settlement.DomainEvents;

using System;
using Shared.DomainDrivenDesign.EventSourcing;

public record SettlementProcessingStartedEvent(Guid SettlementId,
                                               Guid EstateId,
                                               Guid MerchantId,
                                               DateTime ProcessingStartedDateTime) : DomainEvent(SettlementId, Guid.NewGuid());