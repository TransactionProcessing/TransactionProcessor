using Shouldly;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests;

public class VoucherAggregateTests
    {
        [Fact]
        public void VoucherAggregate_CanBeCreated_IsCreated()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);

            aggregate.AggregateId.ShouldBe(TestData.VoucherId);
        }

        [Fact]
        public void VoucherAggregate_Generate_VoucherIsGenerated()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);

            var voucher = aggregate.GetVoucher();

            voucher.IsGenerated.ShouldBeTrue();
            voucher.EstateId.ShouldBe(TestData.EstateId);
            voucher.IsIssued.ShouldBeFalse();
            voucher.GeneratedDateTime.ShouldBe(TestData.GeneratedDateTime);
            voucher.VoucherCode.ShouldNotBeNullOrEmpty();
            voucher.TransactionId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public void VoucherAggregate_Generate_VoucherAlreadyGenerated_ErrorThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
                                                    });
        }

        [Fact]
        public void VoucherAggregate_Generate_VoucherAlreadyIssued_ErrorThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            aggregate.Issue(TestData.RecipientEmail, TestData.RecipientMobile, TestData.IssuedDateTime);
            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.IssuedDateTime, TestData.Value);
                                                    });
        }

        [Fact]
        public void VoucherAggregate_Generate_InvalidOperatorIdentifier_ErrorIsThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);

            Should.Throw<ArgumentNullException>(() =>
                                                {
                                                    aggregate.Generate(Guid.Empty, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
                                                });
        }

        [Fact]
        public void VoucherAggregate_Generate_InvalidEstateId_ErrorIsThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);

            Should.Throw<ArgumentNullException>(() =>
                                                {
                                                    aggregate.Generate(TestData.OperatorId, Guid.Empty, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
                                                });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void VoucherAggregate_Generate_InvalidValue_ErrorIsThrown(Decimal value)
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);

            Should.Throw<ArgumentOutOfRangeException>(() =>
                                                      {
                                                          aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, value);
                                                      });
        }

        [Fact]
        public void VoucherAggregate_Issue_VoucherIsIssued()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            aggregate.Issue(TestData.RecipientEmail, TestData.RecipientMobile, TestData.IssuedDateTime);
            var voucher = aggregate.GetVoucher();
            voucher.IsIssued.ShouldBeTrue();
        }

        [Fact]
        public void VoucherAggregate_Issue_VoucherNotGenerated_ErrorThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Issue(TestData.RecipientEmail, TestData.RecipientMobile, TestData.IssuedDateTime);
                                                    });
        }
        [Fact]
        public void VoucherAggregate_Issue_VoucherAlreadyIssued_ErrorThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            aggregate.Issue(TestData.RecipientEmail, TestData.RecipientMobile, TestData.IssuedDateTime);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Issue(TestData.RecipientEmail, TestData.RecipientMobile, TestData.IssuedDateTime);
                                                    });
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(null, "")]
        public void VoucherAggregate_Issue_EitherEmailOrMobileIsRequired_ErrorThrown(String recipientEmail, String recipientMobile)
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            Should.Throw<ArgumentNullException>(() =>
                                                {
                                                    aggregate.Issue(recipientEmail, recipientMobile, TestData.IssuedDateTime);
                                                });
        }

        [Fact]
        public void VoucherAggregate_AddBarcode_BarcodeIsAdded()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            aggregate.AddBarcode(TestData.Barcode);
            var voucher = aggregate.GetVoucher();
            voucher.Barcode.ShouldBe(TestData.Barcode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void VoucherAggregate_AddBarcode_InvalidBarcode_ErrorThrown(String barcode)
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);

            Should.Throw<ArgumentException>(() =>
                                            {
                                                aggregate.AddBarcode(barcode);
                                            });
        }

        [Fact]
        public void VoucherAggregate_AddBarcode_VoucherNotGenerated_ErrorThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.AddBarcode(TestData.Barcode);
                                                    });
        }

        [Fact]
        public void VoucherAggregate_Redeem_VoucherIsRedeemed()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            aggregate.Issue(TestData.RecipientEmail, TestData.RecipientMobile, TestData.IssuedDateTime);
            aggregate.Redeem(TestData.RedeemedDateTime);

            var voucher = aggregate.GetVoucher();
            voucher.IsRedeemed.ShouldBeTrue();
        }

        [Fact]
        public void VoucherAggregate_Redeem_VoucherNotGenerated_ErrorThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Redeem(TestData.RedeemedDateTime);
                                                    });
        }

        [Fact]
        public void VoucherAggregate_Redeem_VoucherNotIssued_ErrorThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Redeem(TestData.RedeemedDateTime);
                                                    });
        }

        [Fact]
        public void VoucherAggregate_Redeem_VoucherAlreadyRedeemed_ErrorThrown()
        {
            Aggregates.VoucherAggregate aggregate = Aggregates.VoucherAggregate.Create(TestData.VoucherId);
            aggregate.Generate(TestData.OperatorId, TestData.EstateId, TestData.TransactionId, TestData.GeneratedDateTime, TestData.Value);
            aggregate.Issue(TestData.RecipientEmail, TestData.RecipientMobile, TestData.IssuedDateTime);
            aggregate.Redeem(TestData.RedeemedDateTime);
            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        aggregate.Redeem(TestData.RedeemedDateTime);
                                                    });
        }
    }