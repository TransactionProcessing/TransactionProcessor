﻿using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

[ExcludeFromCodeCoverage]
public class SettlementDomainEvents {
    public record MerchantFeeAddedPendingSettlementEvent(Guid SettlementId, Guid EstateId, Guid MerchantId, Guid TransactionId, Decimal CalculatedValue, Int32 FeeCalculationType, Guid FeeId, Decimal FeeValue, DateTime FeeCalculatedDateTime) : DomainEvent(SettlementId, Guid.NewGuid());
    public record MerchantFeeSettledEvent(Guid SettlementId, Guid EstateId, Guid MerchantId, Guid TransactionId, Decimal CalculatedValue, Int32 FeeCalculationType, Guid FeeId, Decimal FeeValue, DateTime FeeCalculatedDateTime, DateTime SettledDateTime) : DomainEvent(SettlementId, Guid.NewGuid());
    public record SettlementCompletedEvent(Guid SettlementId, Guid EstateId, Guid MerchantId) : DomainEvent(SettlementId, Guid.NewGuid());
    public record SettlementCreatedForDateEvent(Guid SettlementId, Guid EstateId, Guid MerchantId, DateTime SettlementDate) : DomainEvent(SettlementId, Guid.NewGuid());
    public record SettlementProcessingStartedEvent(Guid SettlementId, Guid EstateId, Guid MerchantId, DateTime ProcessingStartedDateTime) : DomainEvent(SettlementId, Guid.NewGuid());
}