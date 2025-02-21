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
                      ExpiryDate = @event.ExpiryDateTime.Date,
                      ExpiryDateTime = @event.ExpiryDateTime,
                      GenerateDate = @event.GeneratedDateTime.Date,
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
                      IssuedDate = @event.IssuedDateTime.Date,
                      IssuedDateTime = @event.IssuedDateTime,
                      RecipientEmail = @event.RecipientEmail,
                      RecipientMobile = @event.RecipientMobile
        };


    [Pure]
    public static VoucherState HandleVoucherFullyRedeemedEvent(this VoucherState state, VoucherDomainEvents.VoucherFullyRedeemedEvent @event) =>
        state with{
                      IsFullyRedeemed = true,
                      RedeemedDate = @event.RedeemedDateTime.Date,
                      RedeemedDateTime = @event.RedeemedDateTime,
                      IsRedeemed = true
        };
}

public record VoucherState : Shared.EventStore.ProjectionEngine.State
    {
        public DateTime ExpiryDate { get; init; }
        public DateTime ExpiryDateTime { get; init; }

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

        public DateTime GenerateDateTime { get; init; }

        public DateTime IssuedDateTime { get; init; }

        public DateTime RedeemedDateTime { get; init; }

        public DateTime GenerateDate { get; init; }

        public DateTime IssuedDate { get; init; }

        public DateTime RedeemedDate { get; init; }
        
        public String Barcode { get; init; }

        public Boolean IsFullyRedeemed { get; init; }
    }
}
