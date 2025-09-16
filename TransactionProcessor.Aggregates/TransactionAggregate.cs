using SimpleResults;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;

    public static class TransactionAggregateExtensions{

        public static Result DeclineTransaction(this TransactionAggregate aggregate,
            Guid operatorId,
                                       String operatorResponseCode,
                                       String operatorResponseMessage,
                                       String responseCode,
                                       String responseMessage)
        {
            Result result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyAuthorised();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyDeclined();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.TransactionDeclinedByOperatorEvent transactionDeclinedByOperatorEvent =
                new(aggregate.AggregateId,
                                                       aggregate.EstateId,
                                                       aggregate.MerchantId,
                                                       operatorId,
                                                       operatorResponseCode,
                                                       operatorResponseMessage,
                                                       responseCode,
                                                       responseMessage, 
                                                       aggregate.TransactionDateTime);
            aggregate.ApplyAndAppend(transactionDeclinedByOperatorEvent);

            return Result.Success();
        }

        public static Result DeclineTransactionLocally(this TransactionAggregate aggregate, 
                                                     String responseCode,
                                                     String responseMessage)
        {
            Result result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyAuthorised();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyDeclined();
            if (result.IsFailed)
                return result;
            TransactionDomainEvents.TransactionHasBeenLocallyDeclinedEvent transactionHasBeenLocallyDeclinedEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, responseCode, responseMessage,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(transactionHasBeenLocallyDeclinedEvent);

            return Result.Success();
        }

        public static List<CalculatedFee> GetFees(this TransactionAggregate aggregate)
        {
            return aggregate.CalculatedFees;
        }

        public static TransactionProcessor.Models.Transaction GetTransaction(this TransactionAggregate aggregate)
        {
            return new TransactionProcessor.Models.Transaction
            {
                AuthorisationCode = aggregate.AuthorisationCode,
                MerchantId = aggregate.MerchantId,
                OperatorTransactionId = aggregate.OperatorTransactionId,
                ResponseMessage = aggregate.ResponseMessage,
                TransactionAmount = aggregate.TransactionAmount.HasValue ? aggregate.TransactionAmount.Value : 0,
                TransactionDateTime = aggregate.TransactionDateTime,
                TransactionNumber = aggregate.TransactionNumber,
                TransactionReference = aggregate.TransactionReference,
                OperatorId = aggregate.OperatorId,
                AdditionalRequestMetadata = aggregate.AdditionalTransactionRequestMetadata,
                AdditionalResponseMetadata = aggregate.AdditionalTransactionResponseMetadata,
                ResponseCode = aggregate.ResponseCode,
                IsComplete = aggregate.IsCompleted
            };
        }

        public static Result AddFee(this TransactionAggregate aggregate, CalculatedFee calculatedFee)
        {
            if (aggregate.HasFeeAlreadyBeenAdded(calculatedFee))
                return Result.Success();

            if (calculatedFee == null)
                return Result.Invalid("Calculated Fee must not be null");

            Result result = aggregate.CheckTransactionHasBeenAuthorised();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionHasBeenCompleted();
            if (result.IsFailed)
                return result;
            result =  aggregate.CheckTransactionCanAttractFees();
            if (result.IsFailed)
                return result;

            DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.ServiceProvider)
            {
                // This is an operational (service provider) fee
                @event = new TransactionDomainEvents.ServiceProviderFeeAddedToTransactionEvent(aggregate.AggregateId,
                                                                       aggregate.EstateId,
                                                                       aggregate.MerchantId,
                                                                       calculatedFee.CalculatedValue,
                                                                       (Int32)calculatedFee.FeeCalculationType,
                                                                       calculatedFee.FeeId,
                                                                       calculatedFee.FeeValue,
                                                                       calculatedFee.FeeCalculatedDateTime,
                                                                       aggregate.TransactionDateTime);
            }
            else
            {
                return Result.Invalid("Unsupported Fee Type");
            }

            aggregate.ApplyAndAppend(@event);
            
            return Result.Success();
        }

        public static Result AddProductDetails(this TransactionAggregate aggregate, Guid contractId,
                                             Guid productId)
        {
            if (contractId == Guid.Empty)
                return Result.Invalid($"Contract Id must not be [{Guid.Empty}]");
            if (productId == Guid.Empty)
                return Result.Invalid($"Product Id must not be [{Guid.Empty}]");

            Result result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyCompleted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckProductDetailsNotAlreadyAdded();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.ProductDetailsAddedToTransactionEvent productDetailsAddedToTransactionEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, contractId, productId,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(productDetailsAddedToTransactionEvent);

            return Result.Success();
        }

        public static Result AddFeePendingSettlement(this TransactionAggregate aggregate, CalculatedFee calculatedFee,
                                             DateTime settlementDueDate)
        {
            if (calculatedFee == null)
            {
                return Result.Invalid("Calculated fee cannot be null");
            }

            if (aggregate.HasFeeAlreadyBeenAdded(calculatedFee))
                return Result.Success();

            Result result = aggregate.CheckTransactionHasBeenAuthorised();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionHasBeenCompleted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionCanAttractFees();
            if (result.IsFailed)
                return result;

            DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.Merchant){
                @event = new TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent(aggregate.AggregateId,
                                                                      aggregate.EstateId,
                                                                      aggregate.MerchantId,
                                                                      calculatedFee.CalculatedValue,
                                                                      (Int32)calculatedFee.FeeCalculationType,
                                                                      calculatedFee.FeeId,
                                                                      calculatedFee.FeeValue,
                                                                      calculatedFee.FeeCalculatedDateTime,
                                                                      settlementDueDate,
                                                                      aggregate.TransactionDateTime);
            }
            else
            {
                return Result.Invalid("Unsupported Fee Type");
            }

            aggregate.ApplyAndAppend(@event);

            return Result.Success();
        }

        public static Result AddSettledFee(this TransactionAggregate aggregate, CalculatedFee calculatedFee,
                                         DateTime settledDateTime, Guid settlementId)
        {
            if (calculatedFee == null) {
                return Result.Invalid("Calculated fee cannot be null");
            }

            if (aggregate.HasFeeAlreadyBeenSettled(calculatedFee))
                return Result.Success();

            Result result = aggregate.CheckTransactionHasBeenAuthorised();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionHasBeenCompleted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionCanAttractFees();
            if (result.IsFailed)
                return result;


            DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.Merchant)
            {
                // This is a merchant fee
                @event = new TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent(aggregate.AggregateId,
                                                                       aggregate.EstateId,
                                                                       aggregate.MerchantId,
                                                                       calculatedFee.CalculatedValue,
                                                                       (Int32)calculatedFee.FeeCalculationType,
                                                                       calculatedFee.FeeId,
                                                                       calculatedFee.FeeValue,
                                                                       calculatedFee.FeeCalculatedDateTime,
                                                                       settledDateTime,
                                                                       settlementId,
                                                                       aggregate.TransactionDateTime);
            }
            else
            {
                return Result.Invalid("Unsupported Fee Type");
            }
            
            aggregate.ApplyAndAppend(@event);
            return Result.Success();
        }

        public static Result AddTransactionSource(this TransactionAggregate aggregate, TransactionSource transactionSource)
        {
            if (aggregate.TransactionSource != TransactionSource.NotSet)
                return Result.Success();

            if (Enum.IsDefined(typeof(TransactionSource), transactionSource) == false)
                return Result.Invalid("Transaction Source must be a valid source");

            TransactionDomainEvents.TransactionSourceAddedToTransactionEvent transactionSourceAddedToTransactionEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, (Int32)transactionSource,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(transactionSourceAddedToTransactionEvent);

            return Result.Success();
        }

        public static Result AuthoriseTransaction(this TransactionAggregate aggregate, 
                                                Guid operatorId,
                                                String authorisationCode,
                                                String operatorResponseCode,
                                                String operatorResponseMessage,
                                                String operatorTransactionId,
                                                String responseCode,
                                                String responseMessage)
        {
            Result result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyAuthorised();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.TransactionAuthorisedByOperatorEvent transactionAuthorisedByOperatorEvent = new(aggregate.AggregateId,
                                                                                                                                 aggregate.EstateId,
                                                                                                                                 aggregate.MerchantId,
                                                                                                                                 operatorId,
                                                                                                                                 authorisationCode,
                                                                                                                                 operatorResponseCode,
                                                                                                                                 operatorResponseMessage,
                                                                                                                                 operatorTransactionId,
                                                                                                                                 responseCode,
                                                                                                                                 responseMessage,
                                                                                                                                 aggregate.TransactionDateTime);
            aggregate.ApplyAndAppend(transactionAuthorisedByOperatorEvent);

            return Result.Success();
        }

        public static Result AuthoriseTransactionLocally(this TransactionAggregate aggregate, 
                                                       String authorisationCode,
                                                       String responseCode,
                                                       String responseMessage)
        {
            var result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyAuthorised();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionCanBeLocallyAuthorised();
            if (result.IsFailed)
                return result;
            TransactionDomainEvents.TransactionHasBeenLocallyAuthorisedEvent transactionHasBeenLocallyAuthorisedEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, authorisationCode, responseCode, responseMessage,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(transactionHasBeenLocallyAuthorisedEvent);

            return Result.Success();
        }

        public static Result CompleteTransaction(this TransactionAggregate aggregate)
        {
            Result result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionHasBeenAuthorisedOrDeclined();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyCompleted();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent =
                new(aggregate.AggregateId,
                                                     aggregate.EstateId,
                                                     aggregate.MerchantId,
                                                     aggregate.ResponseCode,
                                                     aggregate.ResponseMessage,
                                                     aggregate.IsAuthorised || aggregate.IsLocallyAuthorised,
                                                     aggregate.TransactionDateTime,
                                                     aggregate.TransactionType != TransactionType.Logon ? aggregate.TransactionAmount : null,
                                                     aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(transactionHasBeenCompletedEvent);

            return Result.Success();
        }

        public static Result RecordTransactionTimings(this TransactionAggregate aggregate,
                                                    DateTime TransactionStartedDateTime,
                                                    DateTime? OperatorCommunicationsStartedEvent,
                                                    DateTime? OperatorCommunicationsCompletedEvent,
                                                    DateTime TransactionCompletedDateTime) {
            TransactionDomainEvents.TransactionTimingsAddedToTransactionEvent transactionTimingsAddedToTransactionEvent =
                new (aggregate.AggregateId,
                     aggregate.EstateId,
                     aggregate.MerchantId,
                     TransactionStartedDateTime,
                     OperatorCommunicationsStartedEvent,
                     OperatorCommunicationsCompletedEvent,
                     TransactionCompletedDateTime);

            aggregate.ApplyAndAppend(transactionTimingsAddedToTransactionEvent);

            return Result.Success();
        }

        public static Result RecordAdditionalRequestData(this TransactionAggregate aggregate, 
                                                       Guid operatorId,
                                                       Dictionary<String, String> additionalTransactionRequestMetadata)
        {
            var result = aggregate.CheckTransactionNotAlreadyCompleted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyAuthorised();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyDeclined();
            if (result.IsFailed)
                return result;
            result= aggregate.CheckAdditionalRequestDataNotAlreadyRecorded();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.AdditionalRequestDataRecordedEvent additionalRequestDataRecordedEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, operatorId, additionalTransactionRequestMetadata,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(additionalRequestDataRecordedEvent);

            return Result.Success();
        }

        public static Result RecordAdditionalResponseData(this TransactionAggregate aggregate,
                                                        Guid operatorId,
                                                        Dictionary<String, String> additionalTransactionResponseMetadata)
        {
            Result result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckAdditionalResponseDataNotAlreadyRecorded();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.AdditionalResponseDataRecordedEvent additionalResponseDataRecordedEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, operatorId, additionalTransactionResponseMetadata,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(additionalResponseDataRecordedEvent);
            return Result.Success();
        }

        public static Result RequestEmailReceipt(this TransactionAggregate aggregate, String customerEmailAddress)
        {
            Result result = aggregate.CheckTransactionHasBeenCompleted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckCustomerHasNotAlreadyRequestedEmailReceipt();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.CustomerEmailReceiptRequestedEvent customerEmailReceiptRequestedEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, customerEmailAddress,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(customerEmailReceiptRequestedEvent);

            return Result.Success();
        }

        public static Result RequestEmailReceiptResend(this TransactionAggregate aggregate)
        {
            var result = aggregate.CheckCustomerHasAlreadyRequestedEmailReceipt();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent customerEmailReceiptResendRequestedEvent =
                new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(customerEmailReceiptResendRequestedEvent);

            return Result.Success();
        }

        public static Result StartTransaction(this TransactionAggregate aggregate,
                                            DateTime transactionDateTime,
                                            String transactionNumber,
                                            TransactionType transactionType,
                                            String transactionReference,
                                            Guid estateId,
                                            Guid merchantId,
                                            String deviceIdentifier,
                                            Decimal? transactionAmount)
        {
            if (transactionDateTime == DateTime.MinValue)
                return Result.Invalid($"Transaction Date Time must not be [{DateTime.MinValue}]");
            if (String.IsNullOrEmpty(transactionNumber))
                return Result.Invalid("Transaction Number must not be null or empty");
            if (String.IsNullOrEmpty(transactionReference))
                return Result.Invalid("Transaction Reference must not be null or empty");

            if (Int32.TryParse(transactionNumber, out Int32 _) == false) {
                return Result.Invalid("Transaction Number must be numeric");
            }

            if (Enum.IsDefined(typeof(TransactionType), transactionType) == false)
                return Result.Invalid("Transaction Type not valid");

            if (estateId == Guid.Empty)
                return Result.Invalid($"Estate Id must not be [{Guid.Empty}]");
            if (merchantId == Guid.Empty)
                return Result.Invalid($"Merchant Id must not be [{Guid.Empty}]");
            if (String.IsNullOrEmpty(deviceIdentifier))
                return Result.Invalid("Device Identifier must not be null or empty");

            Result result = aggregate.CheckTransactionNotAlreadyStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckTransactionNotAlreadyCompleted();
            if (result.IsFailed)
                return result;

            TransactionDomainEvents.TransactionHasStartedEvent transactionHasStartedEvent = new(aggregate.AggregateId,
                                                                                                   estateId,
                                                                                                   merchantId,
                                                                                                   transactionDateTime,
                                                                                                   transactionNumber,
                                                                                                   transactionType.ToString(),
                                                                                                   transactionReference,
                                                                                                   deviceIdentifier,
                                                                                                   transactionAmount);

            aggregate.ApplyAndAppend(transactionHasStartedEvent);

            return Result.Success();
        }

        public static Result RecordCostPrice(this TransactionAggregate aggregate, Decimal unitCost, Decimal totalCost){
            // Don't emit an event when no cost
            if (unitCost == 0 || totalCost == 0)
                return Result.Success();

            Result result = aggregate.CheckTransactionHasBeenStarted();
            if (result.IsFailed)
                return result;

            if (aggregate.UnitCost != null && aggregate.TotalCost != null) {
                return Result.Success();
            }
            
            TransactionDomainEvents.TransactionCostInformationRecordedEvent transactionCostInformationRecordedEvent = new TransactionDomainEvents.TransactionCostInformationRecordedEvent(aggregate.AggregateId,
                                                                                                              aggregate.EstateId,
                                                                                                              aggregate.MerchantId,
                                                                                                              unitCost,
                                                                                                              totalCost,
                                                                                                              aggregate.TransactionDateTime);
            
            aggregate.ApplyAndAppend(transactionCostInformationRecordedEvent);

            return Result.Success();
        }

        private static Result CheckAdditionalRequestDataNotAlreadyRecorded(this TransactionAggregate aggregate)
        {
            if (aggregate.AdditionalTransactionRequestMetadata != null)
            {
                return Result.Invalid("Additional Request Data already recorded");
            }
            return Result.Success();
        }

        private static Result CheckAdditionalResponseDataNotAlreadyRecorded(this TransactionAggregate aggregate)
        {
            if (aggregate.AdditionalTransactionResponseMetadata != null)
            {
                return Result.Invalid("Additional Response Data already recorded");
            }
            return Result.Success();
        }

        private static Result CheckCustomerHasNotAlreadyRequestedEmailReceipt(this TransactionAggregate aggregate)
        {
            if (aggregate.CustomerEmailReceiptHasBeenRequested)
            {
                return Result.Invalid($"Customer Email Receipt already requested for Transaction [{aggregate.AggregateId}]");
            }
            return Result.Success();
        }

        private static Result CheckCustomerHasAlreadyRequestedEmailReceipt(this TransactionAggregate aggregate)
        {
            if (aggregate.CustomerEmailReceiptHasBeenRequested == false)
            {
                return Result.Invalid($"Customer Email Receipt not already requested for Transaction [{aggregate.AggregateId}]");
            }
            return Result.Success();
        }

        private static Result CheckProductDetailsNotAlreadyAdded(this TransactionAggregate aggregate)
        {
            if (aggregate.IsProductDetailsAdded)
            {
                return Result.Invalid("Product details already added");
            }
            return Result.Success();
        }

        private static Result CheckTransactionCanAttractFees(this TransactionAggregate aggregate)
        {
            if (aggregate.TransactionType != TransactionType.Sale)
            {
                return Result.Invalid($"Transactions of type {aggregate.TransactionType} cannot attract fees");
            }
            return Result.Success();
        }

        private static Result CheckTransactionCanBeLocallyAuthorised(this TransactionAggregate aggregate)
        {
            if (aggregate.TransactionType == TransactionType.Sale)
            {
                return Result.Invalid("Sales cannot be locally authorised");
            }
            return Result.Success();
        }

        private static Result CheckTransactionHasBeenAuthorised(this TransactionAggregate aggregate)
        {
            if (aggregate.IsLocallyAuthorised == false && aggregate.IsAuthorised == false)
            {
                return Result.Invalid($"Transaction [{aggregate.AggregateId}] has not been authorised");
            }
            return Result.Success();
        }

        private static Result CheckTransactionHasBeenAuthorisedOrDeclined(this TransactionAggregate aggregate)
        {
            if (aggregate.IsAuthorised == false && aggregate.IsLocallyAuthorised == false && aggregate.IsDeclined == false && aggregate.IsLocallyDeclined == false)
            {
                return Result.Invalid($"Transaction [{aggregate.AggregateId}] has not been authorised or declined");
            }
            return Result.Success();
        }

        private static Result CheckTransactionHasBeenCompleted(this TransactionAggregate aggregate)
        {
            if (aggregate.IsCompleted == false)
            {
                return Result.Invalid($"Transaction [{aggregate.AggregateId}] has not been completed");
            }
            return Result.Success();
        }

        private static Result CheckTransactionHasBeenStarted(this TransactionAggregate aggregate) {
            if (aggregate.IsStarted == false) {
                return Result.Invalid($"Transaction [{aggregate.AggregateId}] has not been started");
            }
            return Result.Success();
        }

        private static Result CheckTransactionNotAlreadyAuthorised(this TransactionAggregate aggregate)
        {
            if (aggregate.IsLocallyAuthorised || aggregate.IsAuthorised)
            {
                String authtype = aggregate.IsLocallyAuthorised ? " locally " : " ";
                return Result.Invalid($"Transaction [{aggregate.AggregateId}] has already been{authtype}authorised");
            }
            return Result.Success();
        }

        private static Result CheckTransactionNotAlreadyCompleted(this TransactionAggregate aggregate)
        {
            if (aggregate.IsCompleted)
            {
                return Result.Invalid($"Transaction Id [{aggregate.AggregateId}] has already been completed");
            }
            return Result.Success();
        }

        private static Result CheckTransactionNotAlreadyDeclined(this TransactionAggregate aggregate)
        {
            if (aggregate.IsLocallyDeclined || aggregate.IsDeclined)
            {
                String authtype = aggregate.IsLocallyDeclined ? " locally " : " ";
                return Result.Invalid($"Transaction [{aggregate.AggregateId}] has already been{authtype}declined");
            }
            return Result.Success();
        }

        private static Result CheckTransactionNotAlreadyStarted(this TransactionAggregate aggregate)
        {
            if (aggregate.IsStarted)
            {
                return Result.Invalid($"Transaction Id [{aggregate.AggregateId}] has already been started");
            }
            return Result.Success();
        }

        private static Boolean HasFeeAlreadyBeenSettled(this TransactionAggregate aggregate, CalculatedFee calculatedFee)
        {
            return aggregate.CalculatedFees.Any(c => c.FeeId == calculatedFee.FeeId && c.IsSettled);
        }

        private static Boolean HasFeeAlreadyBeenAdded(this TransactionAggregate aggregate, CalculatedFee calculatedFee)
        {
            return aggregate.CalculatedFees.Any(c => c.FeeId == calculatedFee.FeeId);
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.CustomerEmailReceiptRequestedEvent domainEvent)
        {
            aggregate.CustomerEmailAddress = domainEvent.CustomerEmailAddress;
            aggregate.CustomerEmailReceiptHasBeenRequested = true;
            aggregate.ReceiptMessageId = domainEvent.EventId;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent domainEvent)
        {
            aggregate.ReceiptResendCount++;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.AdditionalRequestDataRecordedEvent domainEvent)
        {
            aggregate.AdditionalTransactionRequestMetadata = domainEvent.AdditionalTransactionRequestMetadata;
            aggregate.OperatorId = domainEvent.OperatorId;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.AdditionalResponseDataRecordedEvent domainEvent)
        {
            aggregate.AdditionalTransactionResponseMetadata = domainEvent.AdditionalTransactionResponseMetadata;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.TransactionHasStartedEvent domainEvent)
        {
            aggregate.MerchantId = domainEvent.MerchantId;
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.DeviceIdentifier = domainEvent.DeviceIdentifier;
            aggregate.IsStarted = true;
            aggregate.TransactionDateTime = domainEvent.TransactionDateTime;
            aggregate.TransactionNumber = domainEvent.TransactionNumber;
            aggregate.TransactionType = Enum.Parse<TransactionType>(domainEvent.TransactionType);
            aggregate.TransactionReference = domainEvent.TransactionReference;
            aggregate.IsLocallyDeclined = false;
            aggregate.IsDeclined = false;
            aggregate.IsLocallyAuthorised = false;
            aggregate.IsAuthorised = false;
            aggregate.TransactionAmount = domainEvent.TransactionAmount.HasValue ? domainEvent.TransactionAmount : null;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.TransactionHasBeenLocallyAuthorisedEvent domainEvent)
        {
            aggregate.IsLocallyAuthorised = true;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.AuthorisationCode = domainEvent.AuthorisationCode;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.TransactionHasBeenLocallyDeclinedEvent domainEvent)
        {
            aggregate.IsLocallyDeclined = true;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
            aggregate.ResponseCode = domainEvent.ResponseCode;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.TransactionHasBeenCompletedEvent domainEvent)
        {
            //aggregate.IsStarted = false; // Transaction has reached its final state
            aggregate.IsCompleted = true;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.TransactionAuthorisedByOperatorEvent domainEvent)
        {
            aggregate.IsAuthorised = true;
            aggregate.OperatorId = domainEvent.OperatorId;
            aggregate.OperatorResponseCode = domainEvent.OperatorResponseCode;
            aggregate.OperatorResponseMessage = domainEvent.OperatorResponseMessage;
            aggregate.OperatorTransactionId = domainEvent.OperatorTransactionId;
            aggregate.AuthorisationCode = domainEvent.AuthorisationCode;
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.TransactionDeclinedByOperatorEvent domainEvent)
        {
            aggregate.IsDeclined = true;
            aggregate.OperatorId = domainEvent.OperatorId;
            aggregate.OperatorResponseCode = domainEvent.OperatorResponseCode;
            aggregate.OperatorResponseMessage = domainEvent.OperatorResponseMessage;
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.ProductDetailsAddedToTransactionEvent domainEvent)
        {
            aggregate.IsProductDetailsAdded = true;
            aggregate.ContractId = domainEvent.ContractId;
            aggregate.ProductId = domainEvent.ProductId;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.TransactionSourceAddedToTransactionEvent domainEvent)
        {
            aggregate.TransactionSource = (TransactionSource)domainEvent.TransactionSource;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.TransactionCostInformationRecordedEvent domainEvent){
            aggregate.UnitCost = domainEvent.UnitCostValue;
            aggregate.TotalCost = domainEvent.TotalCostValue;
            aggregate.HasCostsCalculated= true;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.SettledMerchantFeeAddedToTransactionEvent domainEvent)
        {
            CalculatedFee fee = aggregate.CalculatedFees.Single(c => c.FeeId == domainEvent.FeeId);
            fee.IsSettled = true;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.MerchantFeePendingSettlementAddedToTransactionEvent domainEvent)
        {
            aggregate.CalculatedFees.Add(new CalculatedFee
                                         {
                                             CalculatedValue = domainEvent.CalculatedValue,
                                             FeeId = domainEvent.FeeId,
                                             FeeType = FeeType.Merchant,
                                             FeeValue = domainEvent.FeeValue,
                                             FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType,
                                             IsSettled = false,
                                             SettlementDueDate = domainEvent.SettlementDueDate,
                                         });
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDomainEvents.ServiceProviderFeeAddedToTransactionEvent domainEvent)
        {
            aggregate.CalculatedFees.Add(new CalculatedFee
                                         {
                                             CalculatedValue = domainEvent.CalculatedValue,
                                             FeeId = domainEvent.FeeId,
                                             FeeType = FeeType.ServiceProvider,
                                             FeeValue = domainEvent.FeeValue,
                                             FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType
                                         });
        }

        public static void PlayEvent(this TransactionAggregate aggregate,
                                     TransactionDomainEvents.TransactionTimingsAddedToTransactionEvent domainEvent) {
            // Nothing to do here, just a marker for the event
        }
    }

    public record TransactionAggregate : Aggregate
    {
        #region Fields

        internal Dictionary<String, String> AdditionalTransactionRequestMetadata;

        internal Dictionary<String, String> AdditionalTransactionResponseMetadata;

        internal readonly List<CalculatedFee> CalculatedFees;

        #endregion

        #region Constructors

        [ExcludeFromCodeCoverage]
        public TransactionAggregate() {
            this.CalculatedFees = new List<CalculatedFee>();
        }

        private TransactionAggregate(Guid aggregateId) {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.CalculatedFees = new List<CalculatedFee>();
        }

        #endregion

        #region Properties

        public String AuthorisationCode { get; internal set; }

        public Guid ContractId { get; internal set; }
        public Guid ReceiptMessageId { get; internal set; }

        public Boolean CustomerEmailReceiptHasBeenRequested { get; internal set; }

        public String CustomerEmailAddress { get; internal set; }

        public String DeviceIdentifier { get; internal set; }

        public Guid EstateId { get; internal set; }

        public Boolean IsAuthorised { get; internal set; }

        public Boolean IsCompleted { get; internal set; }

        public Boolean IsDeclined { get; internal set; }

        public Boolean IsLocallyAuthorised { get; internal set; }

        public Boolean IsLocallyDeclined { get; internal set; }

        public Boolean IsProductDetailsAdded { get; internal set; }

        public Boolean IsStarted { get; internal set; }

        public Guid MerchantId { get; internal set; }

        public Guid OperatorId { get; internal set; }

        public String OperatorResponseCode { get; internal set; }

        public String OperatorResponseMessage { get; internal set; }

        public String OperatorTransactionId { get; internal set; }

        public Guid ProductId { get; internal set; }

        public String ResponseCode { get; internal set; }

        public String ResponseMessage { get; internal set; }

        public Decimal? TransactionAmount { get; internal set; }

        public Decimal? UnitCost { get; internal set; }

        public Decimal? TotalCost { get; internal set; }

        public DateTime TransactionDateTime { get; internal set; }

        public String TransactionNumber { get; internal set; }

        public String TransactionReference { get; internal set; }

        public TransactionSource TransactionSource { get; internal set; }

        public TransactionType TransactionType { get; internal set; }

        public Int32 ReceiptResendCount { get; internal set; }

        public Boolean HasCostsCalculated{ get; internal set; }

        #endregion

        #region Methods

        public static TransactionAggregate Create(Guid aggregateId) {
            return new TransactionAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => TransactionAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata() {
            return new {
                           this.EstateId
                       };
        }

        #endregion
    }
}