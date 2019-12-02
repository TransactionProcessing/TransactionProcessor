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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.IMEINumber);

            transactionAggregate.IsStarted.ShouldBeTrue();
            transactionAggregate.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            transactionAggregate.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            transactionAggregate.TransactionType.ShouldBe(TestData.TransactionType);
            transactionAggregate.EstateId.ShouldBe(TestData.EstateId);
            transactionAggregate.MerchantId.ShouldBe(TestData.MerchantId);
            transactionAggregate.IMEINumber.ShouldBe(TestData.IMEINumber);
        }

        [Fact]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyStarted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.IMEINumber);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              TestData.TransactionType,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.IMEINumber);
                                                    });
        }

        [Fact]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyCompleted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.IMEINumber);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              TestData.TransactionType,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.IMEINumber);
                                                    });
        }

        [Theory]
        [InlineData(false, "0001", "Logon", true, true, "1234567890" )]
        [InlineData(true, "", "Logon", true, true, "1234567890")]
        [InlineData(true, null, "Logon", true, true, "1234567890")]
        [InlineData(true, "ABCD", "Logon", true, true, "1234567890")]
        [InlineData(true, "0001", "", true, true, "1234567890")]
        [InlineData(true, "0001", null, true, true, "1234567890")]
        [InlineData(true, "0001", "Invalid", true, true, "1234567890")]
        [InlineData(true, "0001", "Logon", false, true, "1234567890")]
        [InlineData(true, "0001", "Logon", true, false, "1234567890")]
        [InlineData(true, "0001", "Logon", true, true, "")]
        [InlineData(true, "0001", "Logon", true, true, null)]
        [InlineData(true, "0001", "Logon", true, true, "ABCD")]
        public void TransactionAggregate_StartTransaction_InvalidData_ErrorThrown(Boolean validTransactionDateTime, String transactionNumber, String transactionType, Boolean validEstateId, Boolean validMerchantId, String imeiNumber)
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
                                                                                              imeiNumber);
                                                    });
        }

        [Fact]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionIsAuthorised()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.IMEINumber);

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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.IMEINumber);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
                                                    });
        }

        [Fact]
        public void TransactionAggregate_CompleteTransaction_TransactionIsCompleted()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.IMEINumber);
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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.IMEINumber);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.CompleteTransaction();
                                                    });
        }
        [Fact]
        public void TransactionAggregate_CompleteTransaction_TransactionAlreadyCompleted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, TestData.TransactionType, TestData.EstateId, TestData.MerchantId, TestData.IMEINumber);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.CompleteTransaction();
                                                    });
        }

    }
}
