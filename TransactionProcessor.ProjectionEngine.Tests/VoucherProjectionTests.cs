using Shared.DomainDrivenDesign.EventSourcing;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.ProjectionEngine.Projections;
using TransactionProcessor.ProjectionEngine.State;

namespace TransactionProcessor.ProjectionEngine.Tests
{
    public class VoucherProjectionTests{
        [Theory]
        [InlineData(typeof(VoucherDomainEvents.VoucherGeneratedEvent), true)]
        [InlineData(typeof(VoucherDomainEvents.BarcodeAddedEvent), true)]
        [InlineData(typeof(VoucherDomainEvents.VoucherIssuedEvent), true)]
        [InlineData(typeof(VoucherDomainEvents.VoucherFullyRedeemedEvent), true)]
        [InlineData(typeof(MerchantDomainEvents.AddressAddedEvent), false)]
        public void VoucherProjection_ShouldIHandleEvent_ReturnsExpectedValue(Type eventType, Boolean expectedResult){
            VoucherProjection projection = new VoucherProjection();

            IDomainEvent domainEvent = eventType switch{
                _ when eventType == typeof(VoucherDomainEvents.VoucherGeneratedEvent) => TestData.VoucherGeneratedEvent,
                _ when eventType == typeof(VoucherDomainEvents.BarcodeAddedEvent) => TestData.BarcodeAddedEvent,
                _ when eventType == typeof(VoucherDomainEvents.VoucherIssuedEvent) => TestData.VoucherIssuedEvent,
                _ when eventType == typeof(VoucherDomainEvents.VoucherFullyRedeemedEvent) => TestData.VoucherFullyRedeemedEvent,
                _ => TestData.AddressAddedEvent
            };

            Boolean result = projection.ShouldIHandleEvent(domainEvent);
            result.ShouldBe(expectedResult);
        }

        [Fact]
        public async Task VoucherProjection_Handle_UnSupportedEvent_EventIsHandled(){
            VoucherProjection projection = new VoucherProjection();
            VoucherState state = new VoucherState();
            MerchantDomainEvents.AddressAddedEvent @event = TestData.AddressAddedEvent;

            VoucherState newState = await projection.Handle(state, @event, CancellationToken.None);

            state.Equals(newState).ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherProjection_Handle_VoucherGeneratedEvent_EventIsHandled(){
            VoucherProjection projection = new VoucherProjection();
            VoucherState state = new VoucherState();

            VoucherState newState = await projection.Handle(state, TestData.VoucherGeneratedEvent, CancellationToken.None);
            newState.VoucherCode.ShouldBe(TestData.VoucherGeneratedEvent.VoucherCode);
            newState.EstateId.ShouldBe(TestData.VoucherGeneratedEvent.EstateId);
            newState.TransactionId.ShouldBe(TestData.VoucherGeneratedEvent.TransactionId);
            newState.VoucherId.ShouldBe(TestData.VoucherGeneratedEvent.VoucherId);
            newState.IsGenerated.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherProjection_Handle_BarcodeAddedEvent_EventIsHandled()
        {
            VoucherProjection projection = new VoucherProjection();
            VoucherState state = new VoucherState();
            state = state with{
                                  VoucherCode = TestData.VoucherCode,
                                  EstateId = TestData.EstateId,
                                  TransactionId = TestData.TransactionId,
                                  VoucherId = TestData.VoucherId
                              };

            VoucherState newState = await projection.Handle(state, TestData.BarcodeAddedEvent, CancellationToken.None);
            newState.Barcode.ShouldBe(TestData.BarcodeAddedEvent.Barcode);
        }

        [Fact]
        public async Task VoucherProjection_Handle_VoucherIssuedEvent_EventIsHandled()
        {
            VoucherProjection projection = new VoucherProjection();
            VoucherState state = new VoucherState();
            state = state with
                    {
                        VoucherCode = TestData.VoucherCode,
                        EstateId = TestData.EstateId,
                        TransactionId = TestData.TransactionId,
                        VoucherId = TestData.VoucherId,
                        Barcode = TestData.Barcode
                    };

            VoucherState newState = await projection.Handle(state, TestData.VoucherIssuedEvent, CancellationToken.None);
            newState.IsIssued.ShouldBeTrue();
        }

        [Fact]
        public async Task VoucherProjection_Handle_VoucherFullyRedeemedEvent_EventIsHandled()
        {
            VoucherProjection projection = new VoucherProjection();
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

            VoucherState newState = await projection.Handle(state, TestData.VoucherFullyRedeemedEvent, CancellationToken.None);
            newState.IsFullyRedeemed.ShouldBeTrue();
        }
    }
}
