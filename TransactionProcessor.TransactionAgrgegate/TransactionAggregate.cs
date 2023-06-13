namespace TransactionProcessor.TransactionAggregate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Models;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;
    using Transaction.DomainEvents;

    public static class TransactionAggregateExtensions{

        public static void DeclineTransaction(this TransactionAggregate aggregate,
            String operatorIdentifier,
                                       String operatorResponseCode,
                                       String operatorResponseMessage,
                                       String responseCode,
                                       String responseMessage)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();
            aggregate.CheckTransactionNotAlreadyDeclined();

            TransactionDeclinedByOperatorEvent transactionDeclinedByOperatorEvent =
                new TransactionDeclinedByOperatorEvent(aggregate.AggregateId,
                                                       aggregate.EstateId,
                                                       aggregate.MerchantId,
                                                       operatorIdentifier,
                                                       operatorResponseCode,
                                                       operatorResponseMessage,
                                                       responseCode,
                                                       responseMessage);
            aggregate.ApplyAndAppend(transactionDeclinedByOperatorEvent);
        }

        public static void DeclineTransactionLocally(this TransactionAggregate aggregate, 
                                                     String responseCode,
                                                     String responseMessage)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();
            aggregate.CheckTransactionNotAlreadyDeclined();
            TransactionHasBeenLocallyDeclinedEvent transactionHasBeenLocallyDeclinedEvent =
                new TransactionHasBeenLocallyDeclinedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, responseCode, responseMessage);

            aggregate.ApplyAndAppend(transactionHasBeenLocallyDeclinedEvent);
        }

        public static List<CalculatedFee> GetFees(this TransactionAggregate aggregate)
        {
            return aggregate.CalculatedFees;
        }

        public static Transaction GetTransaction(this TransactionAggregate aggregate)
        {
            return new Transaction
            {
                AuthorisationCode = aggregate.AuthorisationCode,
                MerchantId = aggregate.MerchantId,
                OperatorTransactionId = aggregate.OperatorTransactionId,
                ResponseMessage = aggregate.ResponseMessage,
                TransactionAmount = aggregate.TransactionAmount.HasValue ? aggregate.TransactionAmount.Value : 0,
                TransactionDateTime = aggregate.TransactionDateTime,
                TransactionNumber = aggregate.TransactionNumber,
                TransactionReference = aggregate.TransactionReference,
                OperatorIdentifier = aggregate.OperatorIdentifier,
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
                @event = new ServiceProviderFeeAddedToTransactionEvent(aggregate.AggregateId,
                                                                       aggregate.EstateId,
                                                                       aggregate.MerchantId,
                                                                       calculatedFee.CalculatedValue,
                                                                       (Int32)calculatedFee.FeeCalculationType,
                                                                       calculatedFee.FeeId,
                                                                       calculatedFee.FeeValue,
                                                                       calculatedFee.FeeCalculatedDateTime);
            }
            else
            {
                throw new InvalidOperationException("Unsupported Fee Type");
            }

            if (@event != null)
            {
                aggregate.ApplyAndAppend(@event);
            }
        }

        public static void AddProductDetails(this TransactionAggregate aggregate, Guid contractId,
                                             Guid productId)
        {
            Guard.ThrowIfInvalidGuid(contractId, typeof(ArgumentException), $"Contract Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(productId, typeof(ArgumentException), $"Product Id must not be [{Guid.Empty}]");

            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyCompleted();
            aggregate.CheckProductDetailsNotAlreadyAdded();

            ProductDetailsAddedToTransactionEvent productDetailsAddedToTransactionEvent =
                new ProductDetailsAddedToTransactionEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, contractId, productId);

            aggregate.ApplyAndAppend(productDetailsAddedToTransactionEvent);
        }

        public static void AddSettledFee(this TransactionAggregate aggregate, CalculatedFee calculatedFee,
                                         DateTime settlementDueDate,
                                         DateTime settledDateTime)
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
            if (calculatedFee.FeeType == FeeType.Merchant)
            {
                // This is a merchant fee
                @event = new MerchantFeeAddedToTransactionEvent(aggregate.AggregateId,
                                                                aggregate.EstateId,
                                                                aggregate.MerchantId,
                                                                calculatedFee.CalculatedValue,
                                                                (Int32)calculatedFee.FeeCalculationType,
                                                                calculatedFee.FeeId,
                                                                calculatedFee.FeeValue,
                                                                calculatedFee.FeeCalculatedDateTime,
                                                                settlementDueDate,
                                                                settledDateTime);
            }
            else
            {
                throw new InvalidOperationException("Unsupported Fee Type");
            }

            if (@event != null)
            {
                aggregate.ApplyAndAppend(@event);
            }
        }

        public static void AddTransactionSource(this TransactionAggregate aggregate, TransactionSource transactionSource)
        {
            Guard.ThrowIfInvalidEnum(typeof(TransactionSource), transactionSource, typeof(ArgumentException), "Transaction Source must be a valid source");

            if (aggregate.TransactionSource != TransactionSource.NotSet)
                return;

            TransactionSourceAddedToTransactionEvent transactionSourceAddedToTransactionEvent =
                new TransactionSourceAddedToTransactionEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, (Int32)transactionSource);

            aggregate.ApplyAndAppend(transactionSourceAddedToTransactionEvent);
        }

        public static void AuthoriseTransaction(this TransactionAggregate aggregate, 
                                                String operatorIdentifier,
                                                String authorisationCode,
                                                String operatorResponseCode,
                                                String operatorResponseMessage,
                                                String operatorTransactionId,
                                                String responseCode,
                                                String responseMessage)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();

            TransactionAuthorisedByOperatorEvent transactionAuthorisedByOperatorEvent = new TransactionAuthorisedByOperatorEvent(aggregate.AggregateId,
                                                                                                                                 aggregate.EstateId,
                                                                                                                                 aggregate.MerchantId,
                                                                                                                                 operatorIdentifier,
                                                                                                                                 authorisationCode,
                                                                                                                                 operatorResponseCode,
                                                                                                                                 operatorResponseMessage,
                                                                                                                                 operatorTransactionId,
                                                                                                                                 responseCode,
                                                                                                                                 responseMessage);
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
            TransactionHasBeenLocallyAuthorisedEvent transactionHasBeenLocallyAuthorisedEvent =
                new TransactionHasBeenLocallyAuthorisedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, authorisationCode, responseCode, responseMessage);

            aggregate.ApplyAndAppend(transactionHasBeenLocallyAuthorisedEvent);
        }

        public static void CompleteTransaction(this TransactionAggregate aggregate)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionHasBeenAuthorisedOrDeclined();
            aggregate.CheckTransactionNotAlreadyCompleted();

            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent =
                new TransactionHasBeenCompletedEvent(aggregate.AggregateId,
                                                     aggregate.EstateId,
                                                     aggregate.MerchantId,
                                                     aggregate.ResponseCode,
                                                     aggregate.ResponseMessage,
                                                     aggregate.IsAuthorised || aggregate.IsLocallyAuthorised,
                                                     aggregate.TransactionDateTime,
                                                     aggregate.TransactionType != TransactionType.Logon ? aggregate.TransactionAmount : null);

            aggregate.ApplyAndAppend(transactionHasBeenCompletedEvent);
        }

        public static void RecordAdditionalRequestData(this TransactionAggregate aggregate, 
                                                       String operatorIdentifier,
                                                       Dictionary<String, String> additionalTransactionRequestMetadata)
        {
            aggregate.CheckTransactionNotAlreadyCompleted();
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckTransactionNotAlreadyAuthorised();
            aggregate.CheckTransactionNotAlreadyDeclined();
            aggregate.CheckAdditionalRequestDataNotAlreadyRecorded();

            AdditionalRequestDataRecordedEvent additionalRequestDataRecordedEvent =
                new AdditionalRequestDataRecordedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, operatorIdentifier, additionalTransactionRequestMetadata);

            aggregate.ApplyAndAppend(additionalRequestDataRecordedEvent);
        }

        public static void RecordAdditionalResponseData(this TransactionAggregate aggregate, 
                                                        String operatorIdentifier,
                                                        Dictionary<String, String> additionalTransactionResponseMetadata)
        {
            aggregate.CheckTransactionHasBeenStarted();
            aggregate.CheckAdditionalResponseDataNotAlreadyRecorded();

            AdditionalResponseDataRecordedEvent additionalResponseDataRecordedEvent =
                new AdditionalResponseDataRecordedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, operatorIdentifier, additionalTransactionResponseMetadata);

            aggregate.ApplyAndAppend(additionalResponseDataRecordedEvent);
        }

        public static void RequestEmailReceipt(this TransactionAggregate aggregate, String customerEmailAddress)
        {
            aggregate.CheckTransactionHasBeenCompleted();
            aggregate.CheckCustomerHasNotAlreadyRequestedEmailReceipt();

            CustomerEmailReceiptRequestedEvent customerEmailReceiptRequestedEvent =
                new CustomerEmailReceiptRequestedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, customerEmailAddress);

            aggregate.ApplyAndAppend(customerEmailReceiptRequestedEvent);
        }

        public static void RequestEmailReceiptResend(this TransactionAggregate aggregate)
        {
            aggregate.CheckCustomerHasAlreadyRequestedEmailReceipt();

            CustomerEmailReceiptResendRequestedEvent customerEmailReceiptResendRequestedEvent =
                new CustomerEmailReceiptResendRequestedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId);

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
            TransactionHasStartedEvent transactionHasStartedEvent = new TransactionHasStartedEvent(aggregate.AggregateId,
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

        private static Boolean HasFeeAlreadyBeenAdded(this TransactionAggregate aggregate, CalculatedFee calculatedFee)
        {
            return aggregate.CalculatedFees.Any(c => c.FeeId == calculatedFee.FeeId);
        }

        public static void PlayEvent(this TransactionAggregate aggregate, CustomerEmailReceiptRequestedEvent domainEvent)
        {
            aggregate.CustomerEmailAddress = domainEvent.CustomerEmailAddress;
            aggregate.CustomerEmailReceiptHasBeenRequested = true;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, CustomerEmailReceiptResendRequestedEvent domainEvent)
        {
            aggregate.ReceiptResendCount++;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, AdditionalRequestDataRecordedEvent domainEvent)
        {
            aggregate.AdditionalTransactionRequestMetadata = domainEvent.AdditionalTransactionRequestMetadata;
            aggregate.OperatorIdentifier = domainEvent.OperatorIdentifier;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, AdditionalResponseDataRecordedEvent domainEvent)
        {
            aggregate.AdditionalTransactionResponseMetadata = domainEvent.AdditionalTransactionResponseMetadata;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionHasStartedEvent domainEvent)
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

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionHasBeenLocallyAuthorisedEvent domainEvent)
        {
            aggregate.IsLocallyAuthorised = true;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.AuthorisationCode = domainEvent.AuthorisationCode;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionHasBeenLocallyDeclinedEvent domainEvent)
        {
            aggregate.IsLocallyDeclined = true;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
            aggregate.ResponseCode = domainEvent.ResponseCode;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionHasBeenCompletedEvent domainEvent)
        {
            aggregate.IsStarted = false; // Transaction has reached its final state
            aggregate.IsCompleted = true;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionAuthorisedByOperatorEvent domainEvent)
        {
            aggregate.IsAuthorised = true;
            aggregate.OperatorResponseCode = domainEvent.OperatorResponseCode;
            aggregate.OperatorResponseMessage = domainEvent.OperatorResponseMessage;
            aggregate.OperatorTransactionId = domainEvent.OperatorTransactionId;
            aggregate.AuthorisationCode = domainEvent.AuthorisationCode;
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionDeclinedByOperatorEvent domainEvent)
        {
            aggregate.IsDeclined = true;
            aggregate.OperatorResponseCode = domainEvent.OperatorResponseCode;
            aggregate.OperatorResponseMessage = domainEvent.OperatorResponseMessage;
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, ProductDetailsAddedToTransactionEvent domainEvent)
        {
            aggregate.IsProductDetailsAdded = true;
            aggregate.ContractId = domainEvent.ContractId;
            aggregate.ProductId = domainEvent.ProductId;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, TransactionSourceAddedToTransactionEvent domainEvent)
        {
            aggregate.TransactionSource = (TransactionSource)domainEvent.TransactionSource;
        }

        public static void PlayEvent(this TransactionAggregate aggregate, MerchantFeeAddedToTransactionEvent domainEvent)
        {
            aggregate.CalculatedFees.Add(new CalculatedFee
                                         {
                                             CalculatedValue = domainEvent.CalculatedValue,
                                             FeeId = domainEvent.FeeId,
                                             FeeType = FeeType.Merchant,
                                             FeeValue = domainEvent.FeeValue,
                                             FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType
                                         });
        }

        public static void PlayEvent(this TransactionAggregate aggregate, ServiceProviderFeeAddedToTransactionEvent domainEvent)
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

        public String OperatorIdentifier { get; internal set; }

        public String OperatorResponseCode { get; internal set; }

        public String OperatorResponseMessage { get; internal set; }

        public String OperatorTransactionId { get; internal set; }

        public Guid ProductId { get; internal set; }

        public String ResponseCode { get; internal set; }

        public String ResponseMessage { get; internal set; }

        public Decimal? TransactionAmount { get; internal set; }

        public DateTime TransactionDateTime { get; internal set; }

        public String TransactionNumber { get; internal set; }

        public String TransactionReference { get; internal set; }

        public TransactionSource TransactionSource { get; internal set; }

        public TransactionType TransactionType { get; internal set; }

        public Int32 ReceiptResendCount { get; internal set; }

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