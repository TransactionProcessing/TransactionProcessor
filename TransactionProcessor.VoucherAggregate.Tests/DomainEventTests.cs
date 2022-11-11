namespace TransactionProcessor.VoucherAggregate.Tests
{
    using Shouldly;
    using Testing;
    using Voucher.DomainEvents;
    using Xunit;

    public class DomainEventTests
        {
            [Fact]
            public void VoucherGeneratedEvent_CanBeCreated_IsCreated()
            {
                VoucherGeneratedEvent voucherGeneratedEvent = new VoucherGeneratedEvent(TestData.VoucherId, TestData.EstateId,
                                                                                           TestData.TransactionId, TestData.IssuedDateTime,
                                                                                           TestData.OperatorIdentifier,
                                                                                           TestData.Value, TestData.VoucherCode, TestData.ExpiryDate,
                                                                                           TestData.Message);

                voucherGeneratedEvent.ShouldNotBeNull();
                voucherGeneratedEvent.AggregateId.ShouldBe(TestData.VoucherId);
                voucherGeneratedEvent.EventId.ShouldNotBe(Guid.Empty);
                voucherGeneratedEvent.VoucherId.ShouldBe(TestData.VoucherId);
                voucherGeneratedEvent.EstateId.ShouldBe(TestData.EstateId);
                voucherGeneratedEvent.OperatorIdentifier.ShouldBe(TestData.OperatorIdentifier);
                voucherGeneratedEvent.Value.ShouldBe(TestData.Value);
                voucherGeneratedEvent.VoucherCode.ShouldBe(TestData.VoucherCode);
                voucherGeneratedEvent.ExpiryDateTime.ShouldBe(TestData.ExpiryDate);
                voucherGeneratedEvent.Message.ShouldBe(TestData.Message);
                voucherGeneratedEvent.GeneratedDateTime.ShouldBe(TestData.IssuedDateTime);
            }

            [Fact]
            public void VoucherIssuedEvent_CanBeCreated_IsCreated()
            {
                VoucherIssuedEvent voucherIssuedEvent = new VoucherIssuedEvent(TestData.VoucherId, TestData.EstateId, TestData.IssuedDateTime, TestData.RecipientEmail, TestData.RecipientMobile);

                voucherIssuedEvent.ShouldNotBeNull();
                voucherIssuedEvent.AggregateId.ShouldBe(TestData.VoucherId);
                voucherIssuedEvent.EventId.ShouldNotBe(Guid.Empty);
                voucherIssuedEvent.VoucherId.ShouldBe(TestData.VoucherId);
                voucherIssuedEvent.EstateId.ShouldBe(TestData.EstateId);
                voucherIssuedEvent.RecipientEmail.ShouldBe(TestData.RecipientEmail);
                voucherIssuedEvent.RecipientMobile.ShouldBe(TestData.RecipientMobile);
                voucherIssuedEvent.IssuedDateTime.ShouldBe(TestData.IssuedDateTime);
            }

            [Fact]
            public void BarcodeAddedEvent_CanBeCreated_IsCreated()
            {
                BarcodeAddedEvent barcodeAddedEvent = new BarcodeAddedEvent(TestData.VoucherId, TestData.EstateId, TestData.Barcode);

                barcodeAddedEvent.ShouldNotBeNull();
                barcodeAddedEvent.AggregateId.ShouldBe(TestData.VoucherId);
                barcodeAddedEvent.EventId.ShouldNotBe(Guid.Empty);
                barcodeAddedEvent.VoucherId.ShouldBe(TestData.VoucherId);
                barcodeAddedEvent.EstateId.ShouldBe(TestData.EstateId);
                barcodeAddedEvent.Barcode.ShouldBe(TestData.Barcode);
            }

            [Fact]
            public void VoucherFullyRedeemedEvent_CanBeCreated_IsCreated()
            {
                VoucherFullyRedeemedEvent voucherFullyRedeemedEvent = new VoucherFullyRedeemedEvent(TestData.VoucherId, TestData.EstateId, TestData.RedeemedDateTime);

                voucherFullyRedeemedEvent.ShouldNotBeNull();
                voucherFullyRedeemedEvent.AggregateId.ShouldBe(TestData.VoucherId);
                voucherFullyRedeemedEvent.EventId.ShouldNotBe(Guid.Empty);
                voucherFullyRedeemedEvent.VoucherId.ShouldBe(TestData.VoucherId);
                voucherFullyRedeemedEvent.EstateId.ShouldBe(TestData.EstateId);
                voucherFullyRedeemedEvent.RedeemedDateTime.ShouldBe(TestData.RedeemedDateTime);
            }
        }
}