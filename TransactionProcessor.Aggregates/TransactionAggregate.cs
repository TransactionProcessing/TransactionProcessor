﻿using TransactionProcessor.DomainEvents;
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

        public static void DeclineTransaction(this TransactionAggregate aggregate,
            Guid operatorId,
                                       String operatorResponseCode,
                                       String operatorResponseMessage,
                                       String responseCode,
                                       String responseMessage)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();
            aggregate.CheckTransactionNotAlreadyDeclined();

            TransactionDomainEvents.TransactionDeclinedByOperatorEvent transactionDeclinedByOperatorEvent =
                new TransactionDomainEvents.TransactionDeclinedByOperatorEvent(aggregate.AggregateId,
                                                       aggregate.EstateId,
                                                       aggregate.MerchantId,
                                                       operatorId,
                                                       operatorResponseCode,
                                                       operatorResponseMessage,
                                                       responseCode,
                                                       responseMessage, 
                                                       aggregate.TransactionDateTime);
            aggregate.ApplyAndAppend(transactionDeclinedByOperatorEvent);
        }

        public static void DeclineTransactionLocally(this TransactionAggregate aggregate, 
                                                     String responseCode,
                                                     String responseMessage)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();
            aggregate.CheckTransactionNotAlreadyDeclined();
            TransactionDomainEvents.TransactionHasBeenLocallyDeclinedEvent transactionHasBeenLocallyDeclinedEvent =
                new TransactionDomainEvents.TransactionHasBeenLocallyDeclinedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, responseCode, responseMessage,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(transactionHasBeenLocallyDeclinedEvent);
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

        public static void AddFee(this TransactionAggregate aggregate, CalculatedFee calculatedFee)
        {
            Guard.ThrowIfNull(calculatedFee, nameof(calculatedFee));

            if (aggregate.HasFeeAlreadyBeenAdded(calculatedFee))
                return;

            aggregate.CheckTransactionHasBeenAuthorised();
            aggregate.CheckTransactionHasBeenCompleted();
            aggregate.CheckTransactionCanAttractFees();

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
                throw new InvalidOperationException("Unsupported Fee Type");
            }

            aggregate.ApplyAndAppend(@event);
        }

        public static void AddProductDetails(this TransactionAggregate aggregate, Guid contractId,
                                             Guid productId)
        {
            Guard.ThrowIfInvalidGuid(contractId, typeof(ArgumentException), $"Contract Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(productId, typeof(ArgumentException), $"Product Id must not be [{Guid.Empty}]");

            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyCompleted();
            aggregate.CheckProductDetailsNotAlreadyAdded();

            TransactionDomainEvents.ProductDetailsAddedToTransactionEvent productDetailsAddedToTransactionEvent =
                new TransactionDomainEvents.ProductDetailsAddedToTransactionEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, contractId, productId,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(productDetailsAddedToTransactionEvent);
        }

        public static void AddFeePendingSettlement(this TransactionAggregate aggregate, CalculatedFee calculatedFee,
                                             DateTime settlementDueDate)
        {
            if (calculatedFee == null)
            {
                throw new ArgumentNullException(nameof(calculatedFee));
            }

            if (aggregate.HasFeeAlreadyBeenAdded(calculatedFee))
                return;

            aggregate.CheckTransactionHasBeenAuthorised();
            aggregate.CheckTransactionHasBeenCompleted();
            aggregate.CheckTransactionCanAttractFees();

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
                throw new InvalidOperationException("Unsupported Fee Type");
            }

            aggregate.ApplyAndAppend(@event);
        }

        public static void AddSettledFee(this TransactionAggregate aggregate, CalculatedFee calculatedFee,
                                         DateTime settledDateTime, Guid settlementId)
        {
            if (calculatedFee == null)
            {
                throw new ArgumentNullException(nameof(calculatedFee));
            }

            aggregate.CheckTransactionHasBeenAuthorised();
            aggregate.CheckTransactionHasBeenCompleted();
            aggregate.CheckTransactionCanAttractFees();

            if (aggregate.HasFeeAlreadyBeenSettled(calculatedFee) == true)
                return;

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
                throw new InvalidOperationException("Unsupported Fee Type");
            }
            
            aggregate.ApplyAndAppend(@event);
        }

        public static void AddTransactionSource(this TransactionAggregate aggregate, TransactionSource transactionSource)
        {
            Guard.ThrowIfInvalidEnum(typeof(TransactionSource), transactionSource, typeof(ArgumentException), "Transaction Source must be a valid source");

            if (aggregate.TransactionSource != TransactionSource.NotSet)
                return;

            TransactionDomainEvents.TransactionSourceAddedToTransactionEvent transactionSourceAddedToTransactionEvent =
                new TransactionDomainEvents.TransactionSourceAddedToTransactionEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, (Int32)transactionSource,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(transactionSourceAddedToTransactionEvent);
        }

        public static void AuthoriseTransaction(this TransactionAggregate aggregate, 
                                                Guid operatorId,
                                                String authorisationCode,
                                                String operatorResponseCode,
                                                String operatorResponseMessage,
                                                String operatorTransactionId,
                                                String responseCode,
                                                String responseMessage)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();

            TransactionDomainEvents.TransactionAuthorisedByOperatorEvent transactionAuthorisedByOperatorEvent = new TransactionDomainEvents.TransactionAuthorisedByOperatorEvent(aggregate.AggregateId,
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
        }

        public static void AuthoriseTransactionLocally(this TransactionAggregate aggregate, 
                                                       String authorisationCode,
                                                       String responseCode,
                                                       String responseMessage)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();
            aggregate.CheckTransactionCanBeLocallyAuthorised();
            TransactionDomainEvents.TransactionHasBeenLocallyAuthorisedEvent transactionHasBeenLocallyAuthorisedEvent =
                new TransactionDomainEvents.TransactionHasBeenLocallyAuthorisedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, authorisationCode, responseCode, responseMessage,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(transactionHasBeenLocallyAuthorisedEvent);
        }

        public static void CompleteTransaction(this TransactionAggregate aggregate)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionHasBeenAuthorisedOrDeclined();
            aggregate.CheckTransactionNotAlreadyCompleted();

            TransactionDomainEvents.TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent =
                new TransactionDomainEvents.TransactionHasBeenCompletedEvent(aggregate.AggregateId,
                                                     aggregate.EstateId,
                                                     aggregate.MerchantId,
                                                     aggregate.ResponseCode,
                                                     aggregate.ResponseMessage,
                                                     aggregate.IsAuthorised || aggregate.IsLocallyAuthorised,
                                                     aggregate.TransactionDateTime,
                                                     aggregate.TransactionType != TransactionType.Logon ? aggregate.TransactionAmount : null,
                                                     aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(transactionHasBeenCompletedEvent);
        }

        public static void RecordTransactionTimings(this TransactionAggregate aggregate,
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
        }

        public static void RecordAdditionalRequestData(this TransactionAggregate aggregate, 
                                                       Guid operatorId,
                                                       Dictionary<String, String> additionalTransactionRequestMetadata)
        {
            aggregate.CheckTransactionNotAlreadyCompleted();
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();
            aggregate.CheckTransactionNotAlreadyDeclined();
            aggregate.CheckAdditionalRequestDataNotAlreadyRecorded();

            TransactionDomainEvents.AdditionalRequestDataRecordedEvent additionalRequestDataRecordedEvent =
                new TransactionDomainEvents.AdditionalRequestDataRecordedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, operatorId, additionalTransactionRequestMetadata,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(additionalRequestDataRecordedEvent);
        }

        public static void RecordAdditionalResponseData(this TransactionAggregate aggregate,
                                                        Guid operatorId,
                                                        Dictionary<String, String> additionalTransactionResponseMetadata)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckAdditionalResponseDataNotAlreadyRecorded();

            TransactionDomainEvents.AdditionalResponseDataRecordedEvent additionalResponseDataRecordedEvent =
                new TransactionDomainEvents.AdditionalResponseDataRecordedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, operatorId, additionalTransactionResponseMetadata,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(additionalResponseDataRecordedEvent);
        }

        public static void RequestEmailReceipt(this TransactionAggregate aggregate, String customerEmailAddress)
        {
            aggregate.CheckTransactionHasBeenCompleted();
            aggregate.CheckCustomerHasNotAlreadyRequestedEmailReceipt();

            TransactionDomainEvents.CustomerEmailReceiptRequestedEvent customerEmailReceiptRequestedEvent =
                new TransactionDomainEvents.CustomerEmailReceiptRequestedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, customerEmailAddress,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(customerEmailReceiptRequestedEvent);
        }

        public static void RequestEmailReceiptResend(this TransactionAggregate aggregate)
        {
            aggregate.CheckCustomerHasAlreadyRequestedEmailReceipt();

            TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent customerEmailReceiptResendRequestedEvent =
                new TransactionDomainEvents.CustomerEmailReceiptResendRequestedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId,
                    aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(customerEmailReceiptResendRequestedEvent);
        }

        public static void StartTransaction(this TransactionAggregate aggregate,
                                            DateTime transactionDateTime,
                                            String transactionNumber,
                                            TransactionType transactionType,
                                            String transactionReference,
                                            Guid estateId,
                                            Guid merchantId,
                                            String deviceIdentifier,
                                            Decimal? transactionAmount)
        {
            Guard.ThrowIfInvalidDate(transactionDateTime, typeof(ArgumentException), $"Transaction Date Time must not be [{DateTime.MinValue}]");
            Guard.ThrowIfNullOrEmpty(transactionNumber, typeof(ArgumentException), "Transaction Number must not be null or empty");
            Guard.ThrowIfNullOrEmpty(transactionReference, typeof(ArgumentException), "Transaction Reference must not be null or empty");
            if (Int32.TryParse(transactionNumber, out Int32 txnnumber) == false)
            {
                throw new ArgumentException("Transaction Number must be numeric");
            }

            // Validate the transaction Type
            Guard.ThrowIfInvalidEnum(typeof(TransactionType), transactionType, typeof(ArgumentOutOfRangeException), $"Invalid Transaction Type [{transactionType}]");

            Guard.ThrowIfInvalidGuid(estateId, typeof(ArgumentException), $"Estate Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(merchantId, typeof(ArgumentException), $"Merchant Id must not be [{Guid.Empty}]");
            Guard.ThrowIfNullOrEmpty(deviceIdentifier, typeof(ArgumentException), "Device Identifier must not be null or empty");

            aggregate.CheckTransactionNotAlreadyStarted();
            aggregate.CheckTransactionNotAlreadyCompleted();
            TransactionDomainEvents.TransactionHasStartedEvent transactionHasStartedEvent = new TransactionDomainEvents.TransactionHasStartedEvent(aggregate.AggregateId,
                                                                                                   estateId,
                                                                                                   merchantId,
                                                                                                   transactionDateTime,
                                                                                                   transactionNumber,
                                                                                                   transactionType.ToString(),
                                                                                                   transactionReference,
                                                                                                   deviceIdentifier,
                                                                                                   transactionAmount);

            aggregate.ApplyAndAppend(transactionHasStartedEvent);
        }

        public static void RecordCostPrice(this TransactionAggregate aggregate, Decimal unitCost, Decimal totalCost){
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckCostValuesNotAlreadyRecorded();

            // Dont emit an event when no cost
            if (unitCost == 0 || totalCost == 0)
                return;

            TransactionDomainEvents.TransactionCostInformationRecordedEvent transactionCostInformationRecordedEvent = new TransactionDomainEvents.TransactionCostInformationRecordedEvent(aggregate.AggregateId,
                                                                                                              aggregate.EstateId,
                                                                                                              aggregate.MerchantId,
                                                                                                              unitCost,
                                                                                                              totalCost,
                                                                                                              aggregate.TransactionDateTime);
            
            aggregate.ApplyAndAppend(transactionCostInformationRecordedEvent);
        }

        private static void CheckAdditionalRequestDataNotAlreadyRecorded(this TransactionAggregate aggregate)
        {
            if (aggregate.AdditionalTransactionRequestMetadata != null)
            {
                throw new InvalidOperationException("Additional Request Data already recorded");
            }
        }

        private static void CheckAdditionalResponseDataNotAlreadyRecorded(this TransactionAggregate aggregate)
        {
            if (aggregate.AdditionalTransactionResponseMetadata != null)
            {
                throw new InvalidOperationException("Additional Response Data already recorded");
            }
        }

        private static void CheckCostValuesNotAlreadyRecorded(this TransactionAggregate aggregate)
        {
            if (aggregate.UnitCost != null && aggregate.TotalCost != null)
            {
                throw new InvalidOperationException("Cost information already recorded");
            }
        }

        private static void CheckCustomerHasNotAlreadyRequestedEmailReceipt(this TransactionAggregate aggregate)
        {
            if (aggregate.CustomerEmailReceiptHasBeenRequested)
            {
                throw new InvalidOperationException($"Customer Email Receipt already requested for Transaction [{aggregate.AggregateId}]");
            }
        }

        private static void CheckCustomerHasAlreadyRequestedEmailReceipt(this TransactionAggregate aggregate)
        {
            if (aggregate.CustomerEmailReceiptHasBeenRequested == false)
            {
                throw new InvalidOperationException($"Customer Email Receipt not already requested for Transaction [{aggregate.AggregateId}]");
            }
        }

        private static void CheckProductDetailsNotAlreadyAdded(this TransactionAggregate aggregate)
        {
            if (aggregate.IsProductDetailsAdded)
            {
                throw new InvalidOperationException("Product details already added");
            }
        }

        private static void CheckTransactionCanAttractFees(this TransactionAggregate aggregate)
        {
            if (aggregate.TransactionType != TransactionType.Sale)
            {
                throw new NotSupportedException($"Transactions of type {aggregate.TransactionType} cannot attract fees");
            }
        }

        private static void CheckTransactionCanBeLocallyAuthorised(this TransactionAggregate aggregate)
        {
            if (aggregate.TransactionType == TransactionType.Sale)
            {
                throw new InvalidOperationException("Sales cannot be locally authorised");
            }
        }

        private static void CheckTransactionHasBeenAuthorised(this TransactionAggregate aggregate)
        {
            if (aggregate.IsLocallyAuthorised == false && aggregate.IsAuthorised == false)
            {
                throw new InvalidOperationException($"Transaction [{aggregate.AggregateId}] has not been authorised");
            }
        }

        private static void CheckTransactionHasBeenAuthorisedOrDeclined(this TransactionAggregate aggregate)
        {
            if (aggregate.IsAuthorised == false && aggregate.IsLocallyAuthorised == false && aggregate.IsDeclined == false && aggregate.IsLocallyDeclined == false)
            {
                throw new InvalidOperationException($"Transaction [{aggregate.AggregateId}] has not been authorised or declined");
            }
        }

        private static void CheckTransactionHasBeenCompleted(this TransactionAggregate aggregate)
        {
            if (aggregate.IsCompleted == false)
            {
                throw new InvalidOperationException($"Transaction [{aggregate.AggregateId}] has not been completed");
            }
        }

        private static void CheckTransactionHasBeenStarted(this TransactionAggregate aggregate)
        {
            if (aggregate.IsStarted == false)
            {
                throw new InvalidOperationException($"Transaction [{aggregate.AggregateId}] has not been started");
            }
        }

        private static void CheckTransactionNotAlreadyAuthorised(this TransactionAggregate aggregate)
        {
            if (aggregate.IsLocallyAuthorised || aggregate.IsAuthorised)
            {
                String authtype = aggregate.IsLocallyAuthorised ? " locally " : " ";
                throw new InvalidOperationException($"Transaction [{aggregate.AggregateId}] has already been{authtype}authorised");
            }
        }

        private static void CheckTransactionNotAlreadyCompleted(this TransactionAggregate aggregate)
        {
            if (aggregate.IsCompleted)
            {
                throw new InvalidOperationException($"Transaction Id [{aggregate.AggregateId}] has already been completed");
            }
        }

        private static void CheckTransactionNotAlreadyDeclined(this TransactionAggregate aggregate)
        {
            if (aggregate.IsLocallyDeclined || aggregate.IsDeclined)
            {
                String authtype = aggregate.IsLocallyDeclined ? " locally " : " ";
                throw new InvalidOperationException($"Transaction [{aggregate.AggregateId}] has already been{authtype}declined");
            }
        }

        private static void CheckTransactionNotAlreadyStarted(this TransactionAggregate aggregate)
        {
            if (aggregate.IsStarted)
            {
                throw new InvalidOperationException($"Transaction Id [{aggregate.AggregateId}] has already been started");
            }
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