using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

[ExcludeFromCodeCoverage]
public class ReconciliationDomainEvents {
    public record OverallTotalsRecordedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, Int32 TransactionCount, Decimal TransactionValue, DateTimeOffset TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record ReconciliationHasBeenLocallyAuthorisedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, String ResponseCode, String ResponseMessage, DateTimeOffset TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record ReconciliationHasBeenLocallyDeclinedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, String ResponseCode, String ResponseMessage, DateTimeOffset TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record ReconciliationHasCompletedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, DateTimeOffset TransactionDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
    public record ReconciliationHasStartedEvent(Guid TransactionId, Guid EstateId, Guid MerchantId, DateTimeOffset TransactionDateTime, [property: System.Text.Json.Serialization.JsonConverter(typeof(DateTimeOffsetSecondsConverter))] DateTimeOffset HostDateTime) : DomainEvent(TransactionId, Guid.NewGuid());
}
