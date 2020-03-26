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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType,
                                                  TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            transactionAggregate.IsStarted.ShouldBeTrue();
            transactionAggregate.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            transactionAggregate.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            transactionAggregate.TransactionType.ShouldBe(transactionType);
            transactionAggregate.EstateId.ShouldBe(TestData.EstateId);
            transactionAggregate.MerchantId.ShouldBe(TestData.MerchantId);
            transactionAggregate.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
            transactionAggregate.TransactionReference.ShouldBe(TestData.TransactionReference);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyStarted_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              transactionType,
                                                                                              TestData.TransactionReference,
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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            if (transactionType == TransactionType.Logon)
            { 
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode,
                                                          TestData.OperatorResponseCode,
                                                          TestData.OperatorResponseMessage,
                                                          TestData.OperatorTransactionId,
                                                          TestData.ResponseCode,
                                                          TestData.ResponseMessage);
            }

            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              transactionType,
                                                                                              TestData.TransactionReference,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.DeviceIdentifier);
                                                    });
        }

        [Theory]
        [InlineData(false, "0001", TransactionType.Logon,"ABCDEFG", true, true, "A1234567890" )]
        [InlineData(true, "", TransactionType.Logon, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, null, TransactionType.Logon, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "ABCD", TransactionType.Logon, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", false, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", true, false, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", true, true, "")]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", true, true, null)]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", true, true, "A!234567890")]
        [InlineData(true, "0001", (TransactionType)99, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, "", true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, null, true, true, "A1234567890")]
        [InlineData(false, "0001", TransactionType.Sale, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "", TransactionType.Sale, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, null, TransactionType.Sale, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "ABCD", TransactionType.Sale, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Sale, "ABCDEFG", false, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Sale, "ABCDEFG", true, false, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Sale, "ABCDEFG", true, true, "")]
        [InlineData(true, "0001", TransactionType.Sale, "ABCDEFG", true, true, null)]
        [InlineData(true, "0001", TransactionType.Sale, "ABCDEFG", true, true, "A!234567890")]
        [InlineData(true, "0001", TransactionType.Sale, "", true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Sale, null, true, true, "A1234567890")]
        public void TransactionAggregate_StartTransaction_InvalidData_ErrorThrown(Boolean validTransactionDateTime, String transactionNumber, TransactionType transactionType, String transactionReference, Boolean validEstateId, Boolean validMerchantId, String deviceIdentifier)
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
                                                                                              transactionReference,
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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

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
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionAlreadyAuthorisedLocally_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
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
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionCannotBeLocallyyAuthorised_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

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
        public void TransactionAggregate_AuthoriseTransaction_TransactionIsAuthorised(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            transactionAggregate.IsLocallyAuthorised.ShouldBeFalse();
            transactionAggregate.IsAuthorised.ShouldBeTrue();
            transactionAggregate.AuthorisationCode.ShouldBe(TestData.OperatorAuthorisationCode);
            transactionAggregate.OperatorResponseCode.ShouldBe(TestData.OperatorResponseCode);
            transactionAggregate.OperatorResponseMessage.ShouldBe(TestData.OperatorResponseMessage);
            transactionAggregate.OperatorTransactionId.ShouldBe(TestData.OperatorTransactionId);
        }

        [Fact]
        public void TransactionAggregate_AuthoriseTransaction_TransactionNotStarted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AuthoriseTransaction_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            });
        }
        
        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionIsDeclined(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

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
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyAuthorisedLocally_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyDeclinedLocally_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);

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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.DeclineTransaction(TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.DeclineTransactionLocally(TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransaction_TransactionIsDeclined(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            transactionAggregate.DeclineTransaction(TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);

            transactionAggregate.IsAuthorised.ShouldBeFalse();
            transactionAggregate.IsLocallyAuthorised.ShouldBeFalse();
            transactionAggregate.IsDeclined.ShouldBeTrue();
            transactionAggregate.IsLocallyDeclined.ShouldBeFalse();

            transactionAggregate.OperatorResponseCode.ShouldBe(TestData.DeclinedOperatorResponseCode);
            transactionAggregate.OperatorResponseMessage.ShouldBe(TestData.DeclinedOperatorResponseMessage);
        }

        [Fact]
        public void TransactionAggregate_DeclineTransaction_TransactionNotStarted_ErrorThrown()
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.DeclineTransaction(TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            });
        }

        //[Theory]
        //[InlineData(TransactionType.Logon)]
        //[InlineData(TransactionType.Sale)]
        //public void TransactionAggregate_DeclineTransaction_TransactionAlreadyAuthorisedLocally_ErrorThrown(TransactionType transactionType)
        //{
        //    TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
        //    transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
        //    transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

        //    Should.Throw<InvalidOperationException>(() =>
        //    {
        //        transactionAggregate.DeclineTransaction(TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage);
        //    });
        //}

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransaction_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.DeclineTransaction(TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransaction_TransactionAlreadyDeclinedLocally_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.DeclineTransaction(TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransaction_TransactionAlreadyDeclined_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.DeclineTransaction(TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.DeclineTransaction(TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            });
        }
        
        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_CompleteTransaction_TransactionIsCompleted(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            if (transactionType == TransactionType.Logon)
            {
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

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
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            if (transactionType == TransactionType.Logon)
            {
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.CompleteTransaction();
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_RequestDataRecorded(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);

            Should.NotThrow(() =>
                            {
                                transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
                            });
            
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_TransactionNotStarted_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
                            {
                                transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
                            });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_AdditionalRequestDataAlreadyRecorded_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_AlreadyAuthorised_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);

            if (transactionType == TransactionType.Logon)
            {
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_AlreadyDeclined_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);

            if (transactionType == TransactionType.Logon)
            {
                transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.DeclineTransaction(TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.ResponseCode, TestData.ResponseMessage);
            }

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
                                                    });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_AlreadyCompleted_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
            if (transactionType == TransactionType.Logon)
            {
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }
            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
                                                    });
        }

        //#######


        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalResponseData_ResponseDataRecorded(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            if (transactionType == TransactionType.Logon)
            {
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            Should.NotThrow(() =>
            {
                transactionAggregate.RecordAdditionalResponseData(TestData.AdditionalTransactionMetaData);
            });

        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalResponseData_TransactionNotStarted_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.RecordAdditionalResponseData(TestData.AdditionalTransactionMetaData);
            });
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalResponseData_AdditionalResponseDataAlreadyRecorded_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
            if (transactionType == TransactionType.Logon)
            {
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }
            transactionAggregate.RecordAdditionalResponseData(TestData.AdditionalTransactionMetaData);

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.RecordAdditionalResponseData(TestData.AdditionalTransactionMetaData);
            });
        }
        
        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalResponseData_AlreadyCompleted_ErrorThrown(TransactionType transactionType)
        {
            TransactionAggregate transactionAggregate = TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime, TestData.TransactionNumber, transactionType, TestData.TransactionReference, TestData.EstateId, TestData.MerchantId, TestData.DeviceIdentifier);
            transactionAggregate.RecordAdditionalRequestData(TestData.AdditionalTransactionMetaData);
            if (transactionType == TransactionType.Logon)
            {
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else
            {
                transactionAggregate.AuthoriseTransaction(TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }
            transactionAggregate.RecordAdditionalResponseData(TestData.AdditionalTransactionMetaData);
            transactionAggregate.CompleteTransaction();

            Should.Throw<InvalidOperationException>(() =>
            {
                transactionAggregate.RecordAdditionalResponseData(TestData.AdditionalTransactionMetaData);
            });
        }

    }
}
