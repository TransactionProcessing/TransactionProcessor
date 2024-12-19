using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Voucher.DomainEvents;

namespace TransactionProcessor.ProjectionEngine.State
{
public static class VoucherStateExtension{
    [Pure]
    public static VoucherState HandleVoucherGeneratedEvent(this VoucherState state, VoucherGeneratedEvent @event) =>
        state with{
                      VoucherCode = @event.VoucherCode,
                      VoucherId = @event.VoucherId,
                      TransactionId = @event.TransactionId,
                      EstateId = @event.EstateId,
                      IsGenerated = true
                  };

    [Pure]
    public static VoucherState HandleBarcodeAddedEvent(this VoucherState state, BarcodeAddedEvent @event) =>
        state with{
                      Barcode = @event.Barcode,
                  };

    [Pure]
    public static VoucherState HandleVoucherIssuedEvent(this VoucherState state, VoucherIssuedEvent @event) =>
        state with{
                      IsIssued = true
                  };


    [Pure]
    public static VoucherState HandleVoucherFullyRedeemedEvent(this VoucherState state, VoucherFullyRedeemedEvent @event) =>
        state with{
                      IsFullyRedeemed = true
                  };
}

public record VoucherState : Shared.EventStore.ProjectionEngine.State
    {
        public Guid EstateId { get; set; }
        public Guid VoucherId { get; set; }
        public String VoucherCode { get; set; }
        public Guid TransactionId { get; set; }
        public String Barcode { get; set; }
        public Boolean IsGenerated{ get; set; }
        public Boolean IsIssued { get; set; }
        public Boolean IsFullyRedeemed { get; set; }
    }
}
