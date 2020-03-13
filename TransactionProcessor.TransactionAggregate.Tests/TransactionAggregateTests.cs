using System;
using TransactionProcessor.Testing;
using Xunit;

namespace TransactionProcessor.TransactionAggregate.Tests
{
    using Models;
    using Shouldly;

    public class TransactionAggregateTests
    {
        [Fact]
        public void TransactionAggregate_CanBeCreated_IsCreated()
        {
            TransactionAggregate aggregate = TransactionAggregate.Create(TestData.TransactionId);

            aggregate.AggregateId.ShouldBe(TestData.TransactionId);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_StartTransaction_TransactionIsStarted(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            transactionAggregate.IsStarted.ShouldBeTrue();
            transactionAggregate.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            transactionAggregate.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            transactionAggregate.TransactionType.ShouldBe(transactionType);
            transactionAggregate.EstateId.ShouldBe(TestData.EstateId);
            transactionAggregate.MerchantId.ShouldBe(TestData.MerchantId);
            transactionAggregate.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyStarted_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              transactionType,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.DeviceIdentifier);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyCompleted_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              transactionType,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.DeviceIdentifier);
                                                    });
        }

        [Theory]
        [InlineData(false, "0001", TransactionType.Logon, true, true, "A1234567890" )]
        [InlineData(true, "", TransactionType.Logon, true, true, "A1234567890")]
        [InlineData(true, null, TransactionType.Logon, true, true, "A1234567890")]
        [InlineData(true, "ABCD", TransactionType.Logon, true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, false, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, true, false, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, true, true, "")]
        [InlineData(true, "0001", TransactionType.Logon, true, true, null)]
        [InlineData(true, "0001", TransactionType.Logon, true, true, "A!234567890")]
        [InlineData(true, "0001", (TransactionType)99, true, true, "A1234567890")]
        [InlineData(false, "0001", TransactionType.Sale, true, true, "A1234567890")]
        [InlineData(true, "", TransactionType.Sale, true, true, "A1234567890")]
        [InlineData(true, null, TransactionType.Sale, true, true, "A1234567890")]
        [InlineData(true, "ABCD", TransactionType.Sale, true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Sale, false, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Sale, true, false, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Sale, true, true, "")]
        [InlineData(true, "0001", TransactionType.Sale, true, true, null)]
        [InlineData(true, "0001", TransactionType.Sale, true, true, "A!234567890")]
        public void TransactionAggregate_StartTransaction_InvalidData_ErrorThrown(Boolean validTransactionDateTime, String transactionNumber, TransactionType transactionType, Boolean validEstateId, Boolean validMerchantId, String deviceIdentifier)
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

        [Theory]
        [InlineData(TransactionType.Logon)]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionIsAuthorised(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

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

        [Theory]
        [InlineData(TransactionType.Logon)]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionIsDeclined(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

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

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyDeclined_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_CompleteTransaction_TransactionIsCompleted(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
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

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_CompleteTransaction_TransactionNotAuthorised_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.CompleteTransaction();
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_CompleteTransaction_TransactionAlreadyCompleted_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.CompleteTransaction();
                                                    });
        }

    }
}
