using System;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.TransactionAggregate.Tests
{
    using Shouldly;

    public class TransactionAggregateTests
    {
        [Fact]
        public void TransactionAggregate_CanBeCreated_IsCreated()
        {
            TransactionAggregate aggregate = TransactionAggregate.Create(TestData.TransactionId);

            aggregate.AggregateId.ShouldBe(TestData.TransactionId);
        }

        [Fact]
        public void TransactionAggregate_StartTransaction_TransactionIsStarted()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            transactionAggregate.IsStarted.ShouldBeTrue();
            transactionAggregate.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            transactionAggregate.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            transactionAggregate.TransactionType.ShouldBe(TestData.TransactionType);
            transactionAggregate.EstateId.ShouldBe(TestData.EstateId);
            transactionAggregate.MerchantId.ShouldBe(TestData.MerchantId);
            transactionAggregate.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
        }

        [Fact]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyStarted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              TestData.TransactionType,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.DeviceIdentifier);
                                                    });
        }

        [Fact]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyCompleted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              TestData.TransactionType,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.DeviceIdentifier);
                                                    });
        }

        [Theory]
        [InlineData(false, "0001", "Logon", true, true, "A1234567890" )]
        [InlineData(true, "", "Logon", true, true, "A1234567890")]
        [InlineData(true, null, "Logon", true, true, "A1234567890")]
        [InlineData(true, "ABCD", "Logon", true, true, "A1234567890")]
        [InlineData(true, "0001", "", true, true, "A1234567890")]
        [InlineData(true, "0001", null, true, true, "A1234567890")]
        [InlineData(true, "0001", "Invalid", true, true, "A1234567890")]
        [InlineData(true, "0001", "Logon", false, true, "A1234567890")]
        [InlineData(true, "0001", "Logon", true, false, "A1234567890")]
        [InlineData(true, "0001", "Logon", true, true, "")]
        [InlineData(true, "0001", "Logon", true, true, null)]
        [InlineData(true, "0001", "Logon", true, true, "A!234567890")]
        public void TransactionAggregate_StartTransaction_InvalidData_ErrorThrown(Boolean validTransactionDateTime, String transactionNumber, String transactionType, Boolean validEstateId, Boolean validMerchantId, String deviceIdentifier)
        {
            DateTime transactionDateTime = validTransactionDateTime ? TestData.TransactionDateTime : DateTime.MinValue;
            Guid estateId = validEstateId ? TestData.EstateId : Guid.Empty;
            Guid merchantId = validMerchantId ? TestData.MerchantId : Guid.Empty;
            
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            Should.Throw<ArgumentException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(transactionDateTime,
                                                                                              transactionNumber,
                                                                                              transactionType,
                                                                                              estateId,
                                                                                              merchantId,
                                                                                              deviceIdentifier);
                                                    });
        }

        [Fact]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionIsAuthorised()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            transactionAggregate.IsLocallyAuthorised.ShouldBeTrue();
            transactionAggregate.IsAuthorised.ShouldBeFalse();
            transactionAggregate.AuthorisationCode.ShouldBe(TestData.AuthorisationCode);
            transactionAggregate.ResponseCode.ShouldBe(TestData.ResponseCode);
            transactionAggregate.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

        [Fact]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionNotStarted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionAlreadyAuthorised_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionIsDeclined()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            transactionAggregate.DeclineTransactionLocally(TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);

            transactionAggregate.IsAuthorised.ShouldBeFalse();
            transactionAggregate.IsLocallyAuthorised.ShouldBeFalse();
            transactionAggregate.IsDeclined.ShouldBeFalse();
            transactionAggregate.IsLocallyDeclined.ShouldBeTrue();

            transactionAggregate.ResponseCode.ShouldBe(TestData.DeclinedResponseCode);
            transactionAggregate.ResponseMessage.ShouldBe(TestData.DeclinedResponseMessage);
        }

        [Fact]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionNotStarted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            });
        }

        [Fact]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyAuthorised_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            });
        }

        [Fact]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyDeclined_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void TransactionAggregate_CompleteTransaction_TransactionIsCompleted()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            transactionAggregate.IsStarted.ShouldBeFalse();
            transactionAggregate.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void TransactionAggregate_CompleteTransaction_TransactionNotStarted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            
            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.CompleteTransaction();
                                                    });
        }

        [Fact]
        public void TransactionAggregate_CompleteTransaction_TransactionNotAuthorised_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.CompleteTransaction();
                                                    });
        }
        [Fact]
        public void TransactionAggregate_CompleteTransaction_TransactionAlreadyCompleted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.CompleteTransaction();
                                                    });
        }

    }
}
