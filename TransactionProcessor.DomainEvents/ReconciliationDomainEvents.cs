﻿using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

public class ReconciliationDomainEvents {

    [ExcludeFromCodeCoverage]
    public record OverallTotalsRecordedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Int32 TransactionCount, Decimal TransactionValue, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record ReconciliationHasBeenLocallyAuthorisedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, String ResponseCode, String ResponseMessage, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record ReconciliationHasBeenLocallyDeclinedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, String ResponseCode, String ResponseMessage, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record ReconciliationHasCompletedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());

    [ExcludeFromCodeCoverage]
    public record ReconciliationHasStartedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, DateTime TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
}