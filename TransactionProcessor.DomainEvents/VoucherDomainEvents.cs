using System.Diagnostics.CodeAnalysis;
using Shared.DomainDrivenDesign.EventSourcing;

namespace TransactionProcessor.DomainEvents;

[ExcludeFromCodeCoverage]
public class VoucherDomainEvents {
    public record BarcodeAddedEvent(Guid VoucherId, Guid EstateId, String Barcode) : DomainEvent(VoucherId, Guid.NewGuid());
    public record VoucherFullyRedeemedEvent(Guid VoucherId, Guid EstateId, DateTimeOffset RedeemedDateTime) : DomainEvent(VoucherId, Guid.NewGuid());
    public record VoucherGeneratedEvent(Guid VoucherId, Guid EstateId, Guid TransactionId, DateTimeOffset GeneratedDateTime, Guid OperatorId, Decimal Value, String VoucherCode, DateTimeOffset ExpiryDateTime, String Message) : DomainEvent(VoucherId, Guid.NewGuid());
    public record VoucherIssuedEvent(Guid VoucherId, Guid EstateId, DateTimeOffset IssuedDateTime, String RecipientEmail, String RecipientMobile) : DomainEvent(VoucherId, Guid.NewGuid());
}