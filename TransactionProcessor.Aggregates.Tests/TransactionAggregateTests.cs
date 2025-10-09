using Shouldly;
using SimpleResults;
using TransactionProcessor.Models;
using TransactionProcessor.Models.Contract;
using TransactionProcessor.Testing;

namespace TransactionProcessor.Aggregates.Tests{

    public class TransactionAggregateTests{
        [Fact]
        public void TransactionAggregate_CanBeCreated_IsCreated(){
            Aggregates.TransactionAggregate aggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            aggregate.AggregateId.ShouldBe(TestData.TransactionId);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_StartTransaction_TransactionIsStarted(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            Result result = transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.IsStarted.ShouldBeTrue();
            transactionAggregate.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            transactionAggregate.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            transactionAggregate.TransactionType.ShouldBe(transactionType);
            transactionAggregate.EstateId.ShouldBe(TestData.EstateId);
            transactionAggregate.MerchantId.ShouldBe(TestData.MerchantId);
            transactionAggregate.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
            transactionAggregate.TransactionReference.ShouldBe(TestData.TransactionReference);
            transactionAggregate.TransactionAmount.ShouldBe(TestData.TransactionAmount);

            TransactionProcessor.Models.Transaction transaction = transactionAggregate.GetTransaction();
            transaction.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        public void TransactionAggregate_StartTransaction_NullAmount_TransactionIsStarted(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            Result result = transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  null);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.IsStarted.ShouldBeTrue();
            transactionAggregate.TransactionDateTime.ShouldBe(TestData.TransactionDateTime);
            transactionAggregate.TransactionNumber.ShouldBe(TestData.TransactionNumber);
            transactionAggregate.TransactionType.ShouldBe(transactionType);
            transactionAggregate.EstateId.ShouldBe(TestData.EstateId);
            transactionAggregate.MerchantId.ShouldBe(TestData.MerchantId);
            transactionAggregate.DeviceIdentifier.ShouldBe(TestData.DeviceIdentifier);
            transactionAggregate.TransactionReference.ShouldBe(TestData.TransactionReference);
            transactionAggregate.TransactionAmount.ShouldBeNull();

            TransactionProcessor.Models.Transaction transaction = transactionAggregate.GetTransaction();
            transaction.ShouldNotBeNull();
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyStarted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            Result result = transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              transactionType,
                                                                                              TestData.TransactionReference,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.DeviceIdentifier,
                                                                                              TestData.TransactionAmount);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_StartTransaction_TransactionAlreadyCompleted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId,
                                                          TestData.OperatorAuthorisationCode,
                                                          TestData.OperatorResponseCode,
                                                          TestData.OperatorResponseMessage,
                                                          TestData.OperatorTransactionId,
                                                          TestData.ResponseCode,
                                                          TestData.ResponseMessage);
            }

            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                                                              TestData.TransactionNumber,
                                                                                              transactionType,
                                                                                              TestData.TransactionReference,
                                                                                              TestData.EstateId,
                                                                                              TestData.MerchantId,
                                                                                              TestData.DeviceIdentifier,
                                                                                              TestData.TransactionAmount);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(false, "0001", TransactionType.Logon, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "", TransactionType.Logon, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, null, TransactionType.Logon, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "ABCD", TransactionType.Logon, "ABCDEFG", true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", false, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", true, false, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", true, true, "")]
        [InlineData(true, "0001", TransactionType.Logon, "ABCDEFG", true, true, null)]
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
        [InlineData(true, "0001", TransactionType.Sale, "", true, true, "A1234567890")]
        [InlineData(true, "0001", TransactionType.Sale, null, true, true, "A1234567890")]
        public void TransactionAggregate_StartTransaction_InvalidData_ErrorThrown(Boolean validTransactionDateTime,
                                                                                  String transactionNumber,
                                                                                  TransactionType transactionType,
                                                                                  String transactionReference,
                                                                                  Boolean validEstateId,
                                                                                  Boolean validMerchantId,
                                                                                  String deviceIdentifier) {
            DateTime transactionDateTime = validTransactionDateTime ? TestData.TransactionDateTime : DateTime.MinValue;
            Guid estateId = validEstateId ? TestData.EstateId : Guid.Empty;
            Guid merchantId = validMerchantId ? TestData.MerchantId : Guid.Empty;

            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.StartTransaction(transactionDateTime, transactionNumber, transactionType, transactionReference, estateId, merchantId, deviceIdentifier, TestData.TransactionAmount);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddProductDetails_ProductDetailsAdded(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            Result result = transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.IsProductDetailsAdded.ShouldBeTrue();
            transactionAggregate.ContractId.ShouldBe(TestData.ContractId);
            transactionAggregate.ProductId.ShouldBe(TestData.ProductId);
        }

        [Theory]
        [InlineData(TransactionSource.OnlineSale)]
        [InlineData(TransactionSource.FileImport)]
        public void TransactionAggregate_AddTransactionSource_TransactionSourceAdded(TransactionSource transactionSource){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Sale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            Result result = transactionAggregate.AddTransactionSource(transactionSource);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.TransactionSource.ShouldBe(transactionSource);
        }

        [Fact]
        public void TransactionAggregate_AddTransactionSource_InvalidSource_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Sale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            Result result = transactionAggregate.AddTransactionSource((TransactionSource)99);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionSource.OnlineSale)]
        [InlineData(TransactionSource.FileImport)]
        public void TransactionAggregate_AddTransactionSource_SourceAlreadySet_NoErrorThrown(TransactionSource transactionSource){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Sale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AddTransactionSource(transactionSource);
            Result result = transactionAggregate.AddTransactionSource(transactionSource);
            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddProductDetails_InvalidContractId_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            Result result = transactionAggregate.AddProductDetails(Guid.Empty, TestData.ProductId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddProductDetails_InvalidProductId_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            Result result = transactionAggregate.AddProductDetails(TestData.ContractId, Guid.Empty);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddProductDetails_ProductDetailsAlreadyAdded_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);

            Result result = transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);result.IsFailed.ShouldBeTrue();
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddProductDetails_TransactionNotStarted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddProductDetails_TransactionAlreadyCompleted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);

            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId,
                                                          TestData.OperatorAuthorisationCode,
                                                          TestData.OperatorResponseCode,
                                                          TestData.OperatorResponseMessage,
                                                          TestData.OperatorTransactionId,
                                                          TestData.ResponseCode,
                                                          TestData.ResponseMessage);
            }

            transactionAggregate.CompleteTransaction();


            Result result = transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionIsAuthorised(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            Result result = transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.IsLocallyAuthorised.ShouldBeTrue();
            transactionAggregate.IsAuthorised.ShouldBeFalse();
            transactionAggregate.AuthorisationCode.ShouldBe(TestData.AuthorisationCode);
            transactionAggregate.ResponseCode.ShouldBe(TestData.ResponseCode.ToCodeString());
            transactionAggregate.ResponseMessage.ShouldBe(TestData.ResponseMessage);
        }

        [Fact]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionNotStarted_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionAlreadyAuthorisedLocally_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AuthoriseTransactionLocally_TransactionCannotBeLocallyAuthorised_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);

            Result result = transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode,
                                                                                                         TestData.ResponseCode,
                                                                                                         TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }


        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AuthoriseTransaction_TransactionIsAuthorised(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            Result result = transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.IsLocallyAuthorised.ShouldBeFalse();
            transactionAggregate.IsAuthorised.ShouldBeTrue();
            transactionAggregate.AuthorisationCode.ShouldBe(TestData.OperatorAuthorisationCode);
            transactionAggregate.OperatorResponseCode.ShouldBe(TestData.OperatorResponseCode);
            transactionAggregate.OperatorResponseMessage.ShouldBe(TestData.OperatorResponseMessage);
            transactionAggregate.OperatorTransactionId.ShouldBe(TestData.OperatorTransactionId);
        }

        [Fact]
        public void TransactionAggregate_AuthoriseTransaction_TransactionNotStarted_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AuthoriseTransaction_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionIsDeclined(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            Result result = transactionAggregate.DeclineTransactionLocally(TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.IsAuthorised.ShouldBeFalse();
            transactionAggregate.IsLocallyAuthorised.ShouldBeFalse();
            transactionAggregate.IsDeclined.ShouldBeFalse();
            transactionAggregate.IsLocallyDeclined.ShouldBeTrue();

            transactionAggregate.ResponseCode.ShouldBe(TestData.DeclinedResponseCode.ToCodeString());
            transactionAggregate.ResponseMessage.ShouldBe(TestData.DeclinedResponseMessage);
        }

        [Fact]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionNotStarted_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyAuthorisedLocally_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyDeclinedLocally_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransactionLocally_TransactionAlreadyDeclined_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);

            Result result = transactionAggregate.DeclineTransactionLocally(TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransaction_TransactionIsDeclined(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            Result result = transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.IsAuthorised.ShouldBeFalse();
            transactionAggregate.IsLocallyAuthorised.ShouldBeFalse();
            transactionAggregate.IsDeclined.ShouldBeTrue();
            transactionAggregate.IsLocallyDeclined.ShouldBeFalse();

            transactionAggregate.OperatorResponseCode.ShouldBe(TestData.DeclinedOperatorResponseCode);
            transactionAggregate.OperatorResponseMessage.ShouldBe(TestData.DeclinedOperatorResponseMessage);
        }

        [Fact]
        public void TransactionAggregate_DeclineTransaction_TransactionNotStarted_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
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
        public void TransactionAggregate_DeclineTransaction_TransactionAlreadyAuthorised_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransaction_TransactionAlreadyDeclinedLocally_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_DeclineTransaction_TransactionAlreadyDeclined_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);

            Result result =  transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.DeclinedOperatorResponseCode, TestData.DeclinedOperatorResponseMessage, TestData.DeclinedResponseCode, TestData.DeclinedResponseMessage);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_CompleteTransaction_TransactionIsCompleted(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            transactionAggregate.CompleteTransaction();

            transactionAggregate.IsCompleted.ShouldBeTrue();
        }

        [Fact]
        public void TransactionAggregate_CompleteTransaction_TransactionNotStarted_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.CompleteTransaction();
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_CompleteTransaction_TransactionNotAuthorised_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            Result result = transactionAggregate.CompleteTransaction();
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_CompleteTransaction_TransactionAlreadyCompleted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.CompleteTransaction();
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_RequestDataRecorded(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            Result result = transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_TransactionNotStarted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_AdditionalRequestDataAlreadyRecorded_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());

            Result result = transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_AlreadyAuthorised_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());

            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            Result result = transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_AlreadyDeclined_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());

            if (transactionType == TransactionType.Logon){
                transactionAggregate.DeclineTransactionLocally(TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.ResponseCode, TestData.ResponseMessage);
            }

            Result result = transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalRequestData_AlreadyCompleted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        //#######


        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalResponseData_ResponseDataRecorded(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            Result result = transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsSuccess.ShouldBeTrue();

        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalResponseData_TransactionNotStarted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalResponseData_AdditionalResponseDataAlreadyRecorded_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());

            Result result = transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Logon)]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_RecordAdditionalResponseData_AlreadyCompleted_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            if (transactionType != TransactionType.Logon){
                transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            }

            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            if (transactionType == TransactionType.Logon){
                transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            }
            else{
                transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            }

            transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void TransactionAggregate_RequestEmailReceipt_CustomerEmailReceiptHasBeenRequested(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Sale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.RequestEmailReceipt(TestData.CustomerEmailAddress);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.CustomerEmailReceiptHasBeenRequested.ShouldBeTrue();
            transactionAggregate.CustomerEmailAddress.ShouldBe(TestData.CustomerEmailAddress);
        }

        [Fact]
        public void TransactionAggregate_RequestEmailReceipt_TransactionNotCompleted_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Sale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());

            Result result = transactionAggregate.RequestEmailReceipt(TestData.CustomerEmailAddress);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void TransactionAggregate_RequestEmailReceipt_EmailReceiptAlreadyRequested_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Sale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.CompleteTransaction();

            transactionAggregate.RequestEmailReceipt(TestData.CustomerEmailAddress);

            Result result = transactionAggregate.RequestEmailReceipt(TestData.CustomerEmailAddress);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void TransactionAggregate_RequestEmailReceiptResend_CustomerEmailReceiptHasBeenRequested(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Sale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.CompleteTransaction();

            transactionAggregate.RequestEmailReceipt(TestData.CustomerEmailAddress);
            Result result = transactionAggregate.RequestEmailReceiptResend();
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.ReceiptResendCount.ShouldBe(1);
        }

        [Fact]
        public void TransactionAggregate_RequestEmailReceiptResend_ReceiptNotSent_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Sale,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);
            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.RecordAdditionalRequestData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.RecordAdditionalResponseData(TestData.OperatorId, TestData.AdditionalTransactionMetaDataForMobileTopup());

            var result = transactionAggregate.RequestEmailReceiptResend();
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.ServiceProvider)]
        public void TransactionAggregate_AddFee_FeeDetailsAdded(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            CalculatedFee calculatedFee = this.GetCalculatedFeeToAdd(feeType);

            Result result = transactionAggregate.AddFee(calculatedFee);
            result.IsSuccess.ShouldBeTrue();

            List<CalculatedFee> fees = transactionAggregate.GetFees();

            fees.ShouldHaveSingleItem();
            CalculatedFee calculatedFeeAdded = fees.Single();
            calculatedFeeAdded.FeeId.ShouldBe(calculatedFee.FeeId);
            calculatedFeeAdded.CalculatedValue.ShouldBe(calculatedFee.CalculatedValue);
            calculatedFeeAdded.FeeCalculationType.ShouldBe(calculatedFee.FeeCalculationType);
            calculatedFeeAdded.FeeType.ShouldBe(calculatedFee.FeeType);
            calculatedFeeAdded.FeeValue.ShouldBe(calculatedFee.FeeValue);

        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddFee_NullFee_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddFee(null);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.ServiceProvider)]
        [InlineData(TransactionType.Sale, FeeType.Merchant)]
        public void TransactionAggregate_AddFee_TransactionNotAuthorised_ErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddFee(this.GetCalculatedFeeToAdd(feeType));
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        private CalculatedFee GetCalculatedFeeToAdd(FeeType feeType){
            CalculatedFee calculatedFee = null;
            if (feeType == FeeType.Merchant){
                calculatedFee = TestData.CalculatedFeeMerchantFee();
            }
            else if (feeType == FeeType.ServiceProvider){
                calculatedFee = TestData.CalculatedFeeServiceProviderFee();
            }

            return calculatedFee;
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.ServiceProvider)]
        public void TransactionAggregate_AddFee_TransactionNotCompleted_ErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.AddFee(this.GetCalculatedFeeToAdd(feeType));
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.ServiceProvider)]
        public void TransactionAggregate_AddFee_FeeAlreadyAdded_NoErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();
            transactionAggregate.AddFee(this.GetCalculatedFeeToAdd(feeType));

            Result result = transactionAggregate.AddFee(this.GetCalculatedFeeToAdd(feeType));
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.GetFees().Count.ShouldBe(1);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddFee_UnsupportedFeeType_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddFee(TestData.CalculatedFeeUnsupportedFee);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(FeeType.ServiceProvider)]
        [InlineData(FeeType.Merchant)]
        public void TransactionAggregate_AddFee_LogonTransaction_ErrorThrown(FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Logon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddFee(this.GetCalculatedFeeToAdd(feeType));
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.Merchant)]
        public void TransactionAggregate_AddSettledFee_FeeDetailsAdded(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            CalculatedFee calculatedFee = this.GetCalculatedFeeToAdd(feeType);
            transactionAggregate.AddFeePendingSettlement(calculatedFee, TestData.TransactionFeeSettlementDueDate);
            Result result = transactionAggregate.AddSettledFee(calculatedFee, TestData.SettlementDate,TestData.SettlementAggregateId);
            result.IsSuccess.ShouldBeTrue();

            List<CalculatedFee> fees = transactionAggregate.GetFees();

            fees.ShouldHaveSingleItem();
            CalculatedFee calculatedFeeAdded = fees.Single();
            calculatedFeeAdded.FeeId.ShouldBe(calculatedFee.FeeId);
            calculatedFeeAdded.CalculatedValue.ShouldBe(calculatedFee.CalculatedValue);
            calculatedFeeAdded.FeeCalculationType.ShouldBe(calculatedFee.FeeCalculationType);
            calculatedFeeAdded.FeeType.ShouldBe(calculatedFee.FeeType);
            calculatedFeeAdded.FeeValue.ShouldBe(calculatedFee.FeeValue);

        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddSettledFee_NullFee_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddSettledFee(null, TestData.SettlementDate, TestData.SettlementAggregateId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.Merchant)]
        public void TransactionAggregate_AddSettledFee_TransactionNotAuthorised_ErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddSettledFee(this.GetCalculatedFeeToAdd(feeType), TestData.SettlementDate, TestData.SettlementAggregateId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.Merchant)]
        public void TransactionAggregate_AddSettledFee_TransactionNotCompleted_ErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.AddSettledFee(this.GetCalculatedFeeToAdd(feeType), TestData.SettlementDate, TestData.SettlementAggregateId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.Merchant)]
        public void TransactionAggregate_AddSettledFee_FeeNotAlreadyAdded_NoErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();
            CalculatedFee feeDetails = this.GetCalculatedFeeToAdd(feeType);
            transactionAggregate.AddFeePendingSettlement(feeDetails, TestData.TransactionFeeSettlementDueDate);
            transactionAggregate.AddSettledFee(feeDetails, TestData.SettlementDate, TestData.SettlementAggregateId);

            Result result = transactionAggregate.AddSettledFee(feeDetails, TestData.SettlementDate, TestData.SettlementAggregateId);
            result.IsSuccess.ShouldBeTrue();

            transactionAggregate.GetFees().Count.ShouldBe(1);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddSettledFee_UnsupportedFeeType_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();
            CalculatedFee feeDetails = this.GetCalculatedFeeToAdd(FeeType.Merchant);
            transactionAggregate.AddFeePendingSettlement(feeDetails, TestData.TransactionFeeSettlementDueDate);
            
            Result result = transactionAggregate.AddSettledFee(TestData.CalculatedFeeUnsupportedFee, TestData.SettlementDate, TestData.SettlementAggregateId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(FeeType.Merchant)]
        public void TransactionAggregate_AddSettledFee_LogonTransaction_ErrorThrown(FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Logon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddSettledFee(this.GetCalculatedFeeToAdd(feeType), TestData.SettlementDate, TestData.SettlementAggregateId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.Merchant)]
        public void TransactionAggregate_AddFeePendingSettlement_FeeDetailsAdded(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            CalculatedFee calculatedFee = this.GetCalculatedFeeToAdd(feeType);

            Result result = transactionAggregate.AddFeePendingSettlement(calculatedFee, DateTime.Now);
            result.IsSuccess.ShouldBeTrue();

            List<CalculatedFee> fees = transactionAggregate.GetFees();

            fees.ShouldHaveSingleItem();
            CalculatedFee calculatedFeeAdded = fees.Single();
            calculatedFeeAdded.FeeId.ShouldBe(calculatedFee.FeeId);
            calculatedFeeAdded.CalculatedValue.ShouldBe(calculatedFee.CalculatedValue);
            calculatedFeeAdded.FeeCalculationType.ShouldBe(calculatedFee.FeeCalculationType);
            calculatedFeeAdded.FeeType.ShouldBe(calculatedFee.FeeType);
            calculatedFeeAdded.FeeValue.ShouldBe(calculatedFee.FeeValue);

        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddFeePendingSettlement_NullFee_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddFeePendingSettlement(null, DateTime.Now);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.ServiceProvider)]
        [InlineData(TransactionType.Sale, FeeType.Merchant)]
        public void TransactionAggregate_AddFeePendingSettlement_TransactionNotAuthorised_ErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.DeclineTransaction(TestData.OperatorId, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddFeePendingSettlement(this.GetCalculatedFeeToAdd(feeType), DateTime.Now);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }


        [Theory]
        [InlineData(TransactionType.Sale, FeeType.ServiceProvider)]
        public void TransactionAggregate_AddFeePendingSettlement_TransactionNotCompleted_ErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);

            Result result = transactionAggregate.AddFeePendingSettlement(this.GetCalculatedFeeToAdd(feeType), DateTime.Now);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(TransactionType.Sale, FeeType.Merchant)]
        public void TransactionAggregate_AddFeePendingSettlement_FeeAlreadyAdded_NoErrorThrown(TransactionType transactionType, FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();
            transactionAggregate.AddFeePendingSettlement(this.GetCalculatedFeeToAdd(feeType), DateTime.Now);

            var result = transactionAggregate.AddFeePendingSettlement(this.GetCalculatedFeeToAdd(feeType), DateTime.Now);
            result.IsSuccess.ShouldBeTrue();
            transactionAggregate.GetFees().Count.ShouldBe(1);
        }

        [Theory]
        [InlineData(TransactionType.Sale)]
        public void TransactionAggregate_AddFeePendingSettlement_UnsupportedFeeType_ErrorThrown(TransactionType transactionType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  transactionType,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AddProductDetails(TestData.ContractId, TestData.ProductId);
            transactionAggregate.AuthoriseTransaction(TestData.OperatorId, TestData.OperatorAuthorisationCode, TestData.OperatorResponseCode, TestData.OperatorResponseMessage, TestData.OperatorTransactionId, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddFeePendingSettlement(TestData.CalculatedFeeUnsupportedFee, DateTime.Now);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Theory]
        [InlineData(FeeType.ServiceProvider)]
        [InlineData(FeeType.Merchant)]
        public void TransactionAggregate_AddFeePendingSettlement_LogonTransaction_ErrorThrown(FeeType feeType){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Logon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.AuthoriseTransactionLocally(TestData.AuthorisationCode, TestData.ResponseCode, TestData.ResponseMessage);
            transactionAggregate.CompleteTransaction();

            Result result = transactionAggregate.AddFeePendingSettlement(this.GetCalculatedFeeToAdd(feeType), DateTime.Now);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        public static TheoryData<decimal, decimal, decimal?, decimal?, Boolean> TransactionAggregate_RecordCostPrice_SaleTransaction_CostPriceRecorded_Data =>
            new()
            {
                { 0,0,null,null, false},
                { 0,1,null,null, false },
                { 1,0,null,null, false },
                { 0.9m,90m,0.9m,90m, true},
            };

        [Theory]
        [MemberData(nameof(TransactionAggregate_RecordCostPrice_SaleTransaction_CostPriceRecorded_Data))]
        public void TransactionAggregate_RecordCostPrice_SaleTransaction_CostPriceRecorded(Decimal unitCost, Decimal totalCost, Decimal? expectedUnitCost, Decimal? expectedTotalCost, Boolean expectedCostsCalculated)
        {
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Logon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            Result result = transactionAggregate.RecordCostPrice(unitCost, totalCost);
            result.IsSuccess.ShouldBeTrue();
            
            transactionAggregate.UnitCost.ShouldBe(expectedUnitCost);
            transactionAggregate.TotalCost.ShouldBe(expectedTotalCost);
            transactionAggregate.HasCostsCalculated.ShouldBe(expectedCostsCalculated);
        }

        [Fact]
        public void TransactionAggregate_RecordCostPrice_SaleTransaction_CostAlreadyRecorded(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);
            transactionAggregate.StartTransaction(TestData.TransactionDateTime,
                                                  TestData.TransactionNumber,
                                                  TransactionType.Logon,
                                                  TestData.TransactionReference,
                                                  TestData.EstateId,
                                                  TestData.MerchantId,
                                                  TestData.DeviceIdentifier,
                                                  TestData.TransactionAmount);

            transactionAggregate.RecordCostPrice(TestData.UnitCostPrice, TestData.TotalCostPrice);

            Result result = transactionAggregate.RecordCostPrice(TestData.UnitCostPrice, TestData.TotalCostPrice);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void TransactionAggregate_RecordCostPrice_SaleTransaction_NotStarted_ErrorThrown(){
            Aggregates.TransactionAggregate transactionAggregate = Aggregates.TransactionAggregate.Create(TestData.TransactionId);

            Result result = transactionAggregate.RecordCostPrice(TestData.UnitCostPrice, TestData.TotalCostPrice);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
    }
}
