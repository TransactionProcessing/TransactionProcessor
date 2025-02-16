﻿using System.Diagnostics.Contracts;
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
                      IsGenerated = true
                  };

    [Pure]
    public static VoucherState HandleBarcodeAddedEvent(this VoucherState state, VoucherDomainEvents.BarcodeAddedEvent @event) =>
        state with{
                      Barcode = @event.Barcode,
                  };

    [Pure]
    public static VoucherState HandleVoucherIssuedEvent(this VoucherState state, VoucherDomainEvents.VoucherIssuedEvent @event) =>
        state with{
                      IsIssued = true
                  };


    [Pure]
    public static VoucherState HandleVoucherFullyRedeemedEvent(this VoucherState state, VoucherDomainEvents.VoucherFullyRedeemedEvent @event) =>
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
