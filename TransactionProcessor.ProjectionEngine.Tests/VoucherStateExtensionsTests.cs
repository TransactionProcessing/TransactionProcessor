namespace TransactionProcessor.ProjectionEngine.Tests;

using Shouldly;
using State;

public class VoucherStateExtensionsTests{
    [Fact]
    public async Task VoucherStateExtensions_HandleVoucherGeneratedEvent_StateIsUpdated()
    {
        VoucherState state = new VoucherState();

        VoucherState newState = state.HandleVoucherGeneratedEvent(TestData.VoucherGeneratedEvent);

        newState.VoucherCode.ShouldBe(TestData.VoucherGeneratedEvent.VoucherCode);
        newState.EstateId.ShouldBe(TestData.VoucherGeneratedEvent.EstateId);
        newState.TransactionId.ShouldBe(TestData.VoucherGeneratedEvent.TransactionId);
        newState.VoucherId.ShouldBe(TestData.VoucherGeneratedEvent.VoucherId);
        newState.IsGenerated.ShouldBeTrue();
    }

    [Fact]
    public async Task VoucherStateExtensions_HandleBarcodeAddedEvent_StateIsUpdated()
    {
        VoucherState state = new VoucherState();
        state = state with
                {
                    VoucherCode = TestData.VoucherCode,
                    EstateId = TestData.EstateId,
                    TransactionId = TestData.TransactionId,
                    VoucherId = TestData.VoucherId
                };

        VoucherState newState = state.HandleBarcodeAddedEvent(TestData.BarcodeAddedEvent);
        newState.Barcode.ShouldBe(TestData.BarcodeAddedEvent.Barcode);
    }

    [Fact]
    public async Task VoucherStateExtensions_HandleVoucherIssuedEvent_StateIsUpdated()
    {
        VoucherState state = new VoucherState();
        state = state with
                {
                    VoucherCode = TestData.VoucherCode,
                    EstateId = TestData.EstateId,
                    TransactionId = TestData.TransactionId,
                    VoucherId = TestData.VoucherId,
                    Barcode = TestData.Barcode
                };

        VoucherState newState = state.HandleVoucherIssuedEvent(TestData.VoucherIssuedEvent);
        newState.IsIssued.ShouldBeTrue();
    }

    [Fact]
    public async Task VoucherStateExtensions_HandleVoucherFullyRedeemedEvent_StateIsUpdated()
    {
        VoucherState state = new VoucherState();
        state = state with
                {
                    VoucherCode = TestData.VoucherCode,
                    EstateId = TestData.EstateId,
                    TransactionId = TestData.TransactionId,
                    VoucherId = TestData.VoucherId,
                    Barcode = TestData.Barcode,
                    IsGenerated = true,
                    IsIssued = true
                };

        VoucherState newState = state.HandleVoucherFullyRedeemedEvent(TestData.VoucherFullyRedeemedEvent);
        newState.IsFullyRedeemed.ShouldBeTrue();
    }
}