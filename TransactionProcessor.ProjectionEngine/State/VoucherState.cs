using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.State
{
public static class VoucherStateExtension{
    [Pure]
    public static VoucherState HandleVoucherGeneratedEvent(this VoucherState state, VoucherDomainEvents.VoucherGeneratedEvent @event) =>
        state with{
                      VoucherCode = @event.VoucherCode,
                      VoucherId = @event.VoucherId,
                      TransactionId = @event.TransactionId,
                      EstateId = @event.EstateId,
                      IsGenerated = true,
                      Value = @event.Value,
                      ExpiryDate = DateOnly.FromDateTime(@event.ExpiryDateTime.Date),
                      ExpiryDateTime = @event.ExpiryDateTime,
                      GenerateDate = DateOnly.FromDateTime(@event.GeneratedDateTime.Date),
                      GenerateDateTime = @event.GeneratedDateTime,
                      OperatorIdentifier = @event.OperatorId.ToString(),
        };

    [Pure]
    public static VoucherState HandleBarcodeAddedEvent(this VoucherState state, VoucherDomainEvents.BarcodeAddedEvent @event) =>
        state with{
                      Barcode = @event.Barcode,
                  };

    [Pure]
    public static VoucherState HandleVoucherIssuedEvent(this VoucherState state, VoucherDomainEvents.VoucherIssuedEvent @event) =>
        state with{
                      IsIssued = true,
                      IssuedDate = DateOnly.FromDateTime(@event.IssuedDateTime.Date),
                      IssuedDateTime = @event.IssuedDateTime,
                      RecipientEmail = @event.RecipientEmail,
                      RecipientMobile = @event.RecipientMobile
        };


    [Pure]
    public static VoucherState HandleVoucherFullyRedeemedEvent(this VoucherState state, VoucherDomainEvents.VoucherFullyRedeemedEvent @event) =>
        state with{
                      IsFullyRedeemed = true,
                      RedeemedDate = DateOnly.FromDateTime(@event.RedeemedDateTime.Date),
                      RedeemedDateTime = @event.RedeemedDateTime,
                      IsRedeemed = true
        };
}

public record VoucherState : TransactionProcessing.Shared.EventStore.ProjectionEngine.State
    {
        public DateOnly ExpiryDate { get; init; }
        public DateTimeOffset ExpiryDateTime { get; init; }

        public Boolean IsGenerated { get; init; }

        public Boolean IsIssued { get; init; }

        public Boolean IsRedeemed { get; init; }

        public String? OperatorIdentifier { get; init; }

        public String? RecipientEmail { get; init; }

        public String? RecipientMobile { get; init; }

        public Decimal Value { get; init; }

        public String VoucherCode { get; init; }

        public Guid VoucherId { get; init; }

        public Guid EstateId { get; init; }

        public Guid TransactionId { get; init; }

        public DateTimeOffset GenerateDateTime { get; init; }

        public DateTimeOffset IssuedDateTime { get; init; }

        public DateTimeOffset RedeemedDateTime { get; init; }

        public DateOnly GenerateDate { get; init; }

        public DateOnly IssuedDate { get; init; }

        public DateOnly RedeemedDate { get; init; }
        
        public String Barcode { get; init; }

        public Boolean IsFullyRedeemed { get; init; }
    }
}
