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

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Aggregate" />
    /// <seealso cref="Aggregate" />
    public class TransactionAggregate : Aggregate
    {
        #region Fields

        private Dictionary<String, String> AdditionalTransactionRequestMetadata;

        private Dictionary<String, String> AdditionalTransactionResponseMetadata;

        private readonly List<CalculatedFee> CalculatedFees;

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

        public String AuthorisationCode { get; private set; }

        public Guid ContractId { get; private set; }

        public Boolean CustomerEmailReceiptHasBeenRequested { get; private set; }

        public String DeviceIdentifier { get; private set; }

        public Guid EstateId { get; private set; }

        public Boolean IsAuthorised { get; private set; }

        public Boolean IsCompleted { get; private set; }

        public Boolean IsDeclined { get; private set; }

        public Boolean IsLocallyAuthorised { get; private set; }

        public Boolean IsLocallyDeclined { get; private set; }

        public Boolean IsProductDetailsAdded { get; private set; }

        public Boolean IsStarted { get; private set; }

        public Guid MerchantId { get; private set; }

        public String OperatorIdentifier { get; private set; }

        public String OperatorResponseCode { get; private set; }

        public String OperatorResponseMessage { get; private set; }

        public String OperatorTransactionId { get; private set; }

        public Guid ProductId { get; private set; }

        public String ResponseCode { get; private set; }

        public String ResponseMessage { get; private set; }

        public Decimal? TransactionAmount { get; private set; }

        public DateTime TransactionDateTime { get; private set; }

        public String TransactionNumber { get; private set; }

        public String TransactionReference { get; private set; }

        public TransactionSource TransactionSource { get; private set; }

        public TransactionType TransactionType { get; private set; }

        #endregion

        #region Methods

        public void AddFee(CalculatedFee calculatedFee) {
            Guard.ThrowIfNull(calculatedFee, nameof(calculatedFee));

            if (this.HasFeeAlreadyBeenAdded(calculatedFee))
                return;

            this.CheckTransactionHasBeenAuthorised();
            this.CheckTransactionHasBeenCompleted();
            this.CheckTransactionCanAttractFees();

            DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.ServiceProvider) {
                // This is an operational (service provider) fee
                @event = new ServiceProviderFeeAddedToTransactionEvent(this.AggregateId,
                                                                       this.EstateId,
                                                                       this.MerchantId,
                                                                       calculatedFee.CalculatedValue,
                                                                       (Int32)calculatedFee.FeeCalculationType,
                                                                       calculatedFee.FeeId,
                                                                       calculatedFee.FeeValue,
                                                                       calculatedFee.FeeCalculatedDateTime);
            }
            else {
                throw new InvalidOperationException("Unsupported Fee Type");
            }

            if (@event != null) {
                this.ApplyAndAppend(@event);
            }
        }

        public void AddProductDetails(Guid contractId,
                                      Guid productId) {
            Guard.ThrowIfInvalidGuid(contractId, typeof(ArgumentException), $"Contract Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(productId, typeof(ArgumentException), $"Product Id must not be [{Guid.Empty}]");

            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyCompleted();
            this.CheckProductDetailsNotAlreadyAdded();

            ProductDetailsAddedToTransactionEvent productDetailsAddedToTransactionEvent =
                new ProductDetailsAddedToTransactionEvent(this.AggregateId, this.EstateId, this.MerchantId, contractId, productId);

            this.ApplyAndAppend(productDetailsAddedToTransactionEvent);
        }

        public void AddSettledFee(CalculatedFee calculatedFee,
                                  DateTime settlementDueDate,
                                  DateTime settledDateTime) {
            if (calculatedFee == null) {
                throw new ArgumentNullException(nameof(calculatedFee));
            }

            if (this.HasFeeAlreadyBeenAdded(calculatedFee))
                return;

            this.CheckTransactionHasBeenAuthorised();
            this.CheckTransactionHasBeenCompleted();
            this.CheckTransactionCanAttractFees();

            DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.Merchant) {
                // This is a merchant fee
                @event = new MerchantFeeAddedToTransactionEvent(this.AggregateId,
                                                                this.EstateId,
                                                                this.MerchantId,
                                                                calculatedFee.CalculatedValue,
                                                                (Int32)calculatedFee.FeeCalculationType,
                                                                calculatedFee.FeeId,
                                                                calculatedFee.FeeValue,
                                                                calculatedFee.FeeCalculatedDateTime,
                                                                settlementDueDate,
                                                                settledDateTime);
            }
            else {
                throw new InvalidOperationException("Unsupported Fee Type");
            }

            if (@event != null) {
                this.ApplyAndAppend(@event);
            }
        }

        public void AddTransactionSource(TransactionSource transactionSource) {
            Guard.ThrowIfInvalidEnum(typeof(TransactionSource), transactionSource, typeof(ArgumentException), "Transaction Source must be a valid source");

            if (this.TransactionSource != TransactionSource.NotSet)
                return;

            TransactionSourceAddedToTransactionEvent transactionSourceAddedToTransactionEvent =
                new TransactionSourceAddedToTransactionEvent(this.AggregateId, this.EstateId, this.MerchantId, (Int32)transactionSource);

            this.ApplyAndAppend(transactionSourceAddedToTransactionEvent);
        }

        public void AuthoriseTransaction(String operatorIdentifier,
                                         String authorisationCode,
                                         String operatorResponseCode,
                                         String operatorResponseMessage,
                                         String operatorTransactionId,
                                         String responseCode,
                                         String responseMessage) {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();

            TransactionAuthorisedByOperatorEvent transactionAuthorisedByOperatorEvent = new TransactionAuthorisedByOperatorEvent(this.AggregateId,
                this.EstateId,
                this.MerchantId,
                operatorIdentifier,
                authorisationCode,
                operatorResponseCode,
                operatorResponseMessage,
                operatorTransactionId,
                responseCode,
                responseMessage);
            this.ApplyAndAppend(transactionAuthorisedByOperatorEvent);
        }

        public void AuthoriseTransactionLocally(String authorisationCode,
                                                String responseCode,
                                                String responseMessage) {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();
            this.CheckTransactionCanBeLocallyAuthorised();
            TransactionHasBeenLocallyAuthorisedEvent transactionHasBeenLocallyAuthorisedEvent =
                new TransactionHasBeenLocallyAuthorisedEvent(this.AggregateId, this.EstateId, this.MerchantId, authorisationCode, responseCode, responseMessage);

            this.ApplyAndAppend(transactionHasBeenLocallyAuthorisedEvent);
        }

        public void CompleteTransaction() {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionHasBeenAuthorisedOrDeclined();
            this.CheckTransactionNotAlreadyCompleted();

            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent =
                new TransactionHasBeenCompletedEvent(this.AggregateId,
                                                     this.EstateId,
                                                     this.MerchantId,
                                                     this.ResponseCode,
                                                     this.ResponseMessage,
                                                     this.IsAuthorised || this.IsLocallyAuthorised,
                                                     this.TransactionDateTime,
                                                     this.TransactionType != TransactionType.Logon ? this.TransactionAmount : null);

            this.ApplyAndAppend(transactionHasBeenCompletedEvent);
        }

        public static TransactionAggregate Create(Guid aggregateId) {
            return new TransactionAggregate(aggregateId);
        }

        public void DeclineTransaction(String operatorIdentifier,
                                       String operatorResponseCode,
                                       String operatorResponseMessage,
                                       String responseCode,
                                       String responseMessage) {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();
            this.CheckTransactionNotAlreadyDeclined();

            TransactionDeclinedByOperatorEvent transactionDeclinedByOperatorEvent =
                new TransactionDeclinedByOperatorEvent(this.AggregateId,
                                                       this.EstateId,
                                                       this.MerchantId,
                                                       operatorIdentifier,
                                                       operatorResponseCode,
                                                       operatorResponseMessage,
                                                       responseCode,
                                                       responseMessage);
            this.ApplyAndAppend(transactionDeclinedByOperatorEvent);
        }

        public void DeclineTransactionLocally(String responseCode,
                                              String responseMessage) {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();
            this.CheckTransactionNotAlreadyDeclined();
            TransactionHasBeenLocallyDeclinedEvent transactionHasBeenLocallyDeclinedEvent =
                new TransactionHasBeenLocallyDeclinedEvent(this.AggregateId, this.EstateId, this.MerchantId, responseCode, responseMessage);

            this.ApplyAndAppend(transactionHasBeenLocallyDeclinedEvent);
        }

        public List<CalculatedFee> GetFees() {
            return this.CalculatedFees;
        }

        public Transaction GetTransaction() {
            return new Transaction {
                                       AuthorisationCode = this.AuthorisationCode,
                                       MerchantId = this.MerchantId,
                                       OperatorTransactionId = this.OperatorTransactionId,
                                       ResponseMessage = this.ResponseMessage,
                                       TransactionAmount = this.TransactionAmount.HasValue ? this.TransactionAmount.Value : 0,
                                       TransactionDateTime = this.TransactionDateTime,
                                       TransactionNumber = this.TransactionNumber,
                                       TransactionReference = this.TransactionReference,
                                       OperatorIdentifier = this.OperatorIdentifier,
                                       AdditionalRequestMetadata = this.AdditionalTransactionRequestMetadata,
                                       AdditionalResponseMetadata = this.AdditionalTransactionResponseMetadata,
                                       ResponseCode = this.ResponseCode
                                   };
        }

        public override void PlayEvent(IDomainEvent domainEvent) {
            this.PlayEvent((dynamic)domainEvent);
        }

        public void RecordAdditionalRequestData(String operatorIdentifier,
                                                Dictionary<String, String> additionalTransactionRequestMetadata) {
            this.CheckTransactionNotAlreadyCompleted();
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();
            this.CheckTransactionNotAlreadyDeclined();
            this.CheckAdditionalRequestDataNotAlreadyRecorded();

            AdditionalRequestDataRecordedEvent additionalRequestDataRecordedEvent =
                new AdditionalRequestDataRecordedEvent(this.AggregateId, this.EstateId, this.MerchantId, operatorIdentifier, additionalTransactionRequestMetadata);

            this.ApplyAndAppend(additionalRequestDataRecordedEvent);
        }

        public void RecordAdditionalResponseData(String operatorIdentifier,
                                                 Dictionary<String, String> additionalTransactionResponseMetadata) {
            this.CheckTransactionHasBeenStarted();
            this.CheckAdditionalResponseDataNotAlreadyRecorded();

            AdditionalResponseDataRecordedEvent additionalResponseDataRecordedEvent =
                new AdditionalResponseDataRecordedEvent(this.AggregateId, this.EstateId, this.MerchantId, operatorIdentifier, additionalTransactionResponseMetadata);

            this.ApplyAndAppend(additionalResponseDataRecordedEvent);
        }

        public void RequestEmailReceipt(String customerEmailAddress) {
            this.CheckTransactionHasBeenCompleted();
            this.CheckCustomerHasNotAlreadyRequestedEmailReceipt();

            CustomerEmailReceiptRequestedEvent customerEmailReceiptRequestedEvent =
                new CustomerEmailReceiptRequestedEvent(this.AggregateId, this.EstateId, this.MerchantId, customerEmailAddress);

            this.ApplyAndAppend(customerEmailReceiptRequestedEvent);
        }

        public void StartTransaction(DateTime transactionDateTime,
                                     String transactionNumber,
                                     TransactionType transactionType,
                                     String transactionReference,
                                     Guid estateId,
                                     Guid merchantId,
                                     String deviceIdentifier,
                                     Decimal? transactionAmount) {
            Guard.ThrowIfInvalidDate(transactionDateTime, typeof(ArgumentException), $"Transaction Date Time must not be [{DateTime.MinValue}]");
            Guard.ThrowIfNullOrEmpty(transactionNumber, typeof(ArgumentException), "Transaction Number must not be null or empty");
            Guard.ThrowIfNullOrEmpty(transactionReference, typeof(ArgumentException), "Transaction Reference must not be null or empty");
            if (Int32.TryParse(transactionNumber, out Int32 txnnumber) == false) {
                throw new ArgumentException("Transaction Number must be numeric");
            }

            // Validate the transaction Type
            Guard.ThrowIfInvalidEnum(typeof(TransactionType), transactionType, typeof(ArgumentOutOfRangeException), $"Invalid Transaction Type [{transactionType}]");

            Guard.ThrowIfInvalidGuid(estateId, typeof(ArgumentException), $"Estate Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(merchantId, typeof(ArgumentException), $"Merchant Id must not be [{Guid.Empty}]");
            Guard.ThrowIfNullOrEmpty(deviceIdentifier, typeof(ArgumentException), "Device Identifier must not be null or empty");

            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (r.IsMatch(deviceIdentifier) == false) {
                throw new ArgumentException("Device Identifier must be alphanumeric");
            }

            this.CheckTransactionNotAlreadyStarted();
            this.CheckTransactionNotAlreadyCompleted();
            TransactionHasStartedEvent transactionHasStartedEvent = new TransactionHasStartedEvent(this.AggregateId,
                                                                                                   estateId,
                                                                                                   merchantId,
                                                                                                   transactionDateTime,
                                                                                                   transactionNumber,
                                                                                                   transactionType.ToString(),
                                                                                                   transactionReference,
                                                                                                   deviceIdentifier,
                                                                                                   transactionAmount);

            this.ApplyAndAppend(transactionHasStartedEvent);
        }

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata() {
            return new {
                           this.EstateId
                       };
        }

        private void CheckAdditionalRequestDataNotAlreadyRecorded() {
            if (this.AdditionalTransactionRequestMetadata != null) {
                throw new InvalidOperationException("Additional Request Data already recorded");
            }
        }

        private void CheckAdditionalResponseDataNotAlreadyRecorded() {
            if (this.AdditionalTransactionResponseMetadata != null) {
                throw new InvalidOperationException("Additional Response Data already recorded");
            }
        }

        private void CheckCustomerHasNotAlreadyRequestedEmailReceipt() {
            if (this.CustomerEmailReceiptHasBeenRequested) {
                throw new InvalidOperationException($"Customer Email Receipt already requested for Transaction [{this.AggregateId}]");
            }
        }

        private void CheckProductDetailsNotAlreadyAdded() {
            if (this.IsProductDetailsAdded) {
                throw new InvalidOperationException("Product details already added");
            }
        }

        private void CheckTransactionCanAttractFees() {
            if (this.TransactionType != TransactionType.Sale) {
                throw new NotSupportedException($"Transactions of type {this.TransactionType} cannot attract fees");
            }
        }

        private void CheckTransactionCanBeLocallyAuthorised() {
            if (this.TransactionType == TransactionType.Sale) {
                throw new InvalidOperationException("Sales cannot be locally authorised");
            }
        }

        private void CheckTransactionHasBeenAuthorised() {
            if (this.IsLocallyAuthorised == false && this.IsAuthorised == false) {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been authorised");
            }
        }

        private void CheckTransactionHasBeenAuthorisedOrDeclined() {
            if (this.IsAuthorised == false && this.IsLocallyAuthorised == false && this.IsDeclined == false && this.IsLocallyDeclined == false) {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been authorised or declined");
            }
        }

        private void CheckTransactionHasBeenCompleted() {
            if (this.IsCompleted == false) {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been completed");
            }
        }

        private void CheckTransactionHasBeenStarted() {
            if (this.IsStarted == false) {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been started");
            }
        }

        private void CheckTransactionNotAlreadyAuthorised() {
            if (this.IsLocallyAuthorised || this.IsAuthorised) {
                String authtype = this.IsLocallyAuthorised ? " locally " : " ";
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has already been{authtype}authorised");
            }
        }

        private void CheckTransactionNotAlreadyCompleted() {
            if (this.IsCompleted) {
                throw new InvalidOperationException($"Transaction Id [{this.AggregateId}] has already been completed");
            }
        }

        private void CheckTransactionNotAlreadyDeclined() {
            if (this.IsLocallyDeclined || this.IsDeclined) {
                String authtype = this.IsLocallyDeclined ? " locally " : " ";
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has already been{authtype}declined");
            }
        }

        private void CheckTransactionNotAlreadyStarted() {
            if (this.IsStarted) {
                throw new InvalidOperationException($"Transaction Id [{this.AggregateId}] has already been started");
            }
        }

        private Boolean HasFeeAlreadyBeenAdded(CalculatedFee calculatedFee) {
            return this.CalculatedFees.Any(c => c.FeeId == calculatedFee.FeeId);
        }

        private void PlayEvent(CustomerEmailReceiptRequestedEvent domainEvent) {
            this.CustomerEmailReceiptHasBeenRequested = true;
        }

        private void PlayEvent(AdditionalRequestDataRecordedEvent domainEvent) {
            this.AdditionalTransactionRequestMetadata = domainEvent.AdditionalTransactionRequestMetadata;
            this.OperatorIdentifier = domainEvent.OperatorIdentifier;
        }

        private void PlayEvent(AdditionalResponseDataRecordedEvent domainEvent) {
            this.AdditionalTransactionResponseMetadata = domainEvent.AdditionalTransactionResponseMetadata;
        }

        private void PlayEvent(TransactionHasStartedEvent domainEvent) {
            this.MerchantId = domainEvent.MerchantId;
            this.EstateId = domainEvent.EstateId;
            this.DeviceIdentifier = domainEvent.DeviceIdentifier;
            this.IsStarted = true;
            this.TransactionDateTime = domainEvent.TransactionDateTime;
            this.TransactionNumber = domainEvent.TransactionNumber;
            this.TransactionType = Enum.Parse<TransactionType>(domainEvent.TransactionType);
            this.TransactionReference = domainEvent.TransactionReference;
            this.IsLocallyDeclined = false;
            this.IsDeclined = false;
            this.IsLocallyAuthorised = false;
            this.IsAuthorised = false;
            this.TransactionAmount = domainEvent.TransactionAmount.HasValue ? domainEvent.TransactionAmount : null;
        }

        private void PlayEvent(TransactionHasBeenLocallyAuthorisedEvent domainEvent) {
            this.IsLocallyAuthorised = true;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.ResponseCode = domainEvent.ResponseCode;
            this.AuthorisationCode = domainEvent.AuthorisationCode;
        }

        private void PlayEvent(TransactionHasBeenLocallyDeclinedEvent domainEvent) {
            this.IsLocallyDeclined = true;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.ResponseCode = domainEvent.ResponseCode;
        }

        private void PlayEvent(TransactionHasBeenCompletedEvent domainEvent) {
            this.IsStarted = false; // Transaction has reached its final state
            this.IsCompleted = true;
        }

        private void PlayEvent(TransactionAuthorisedByOperatorEvent domainEvent) {
            this.IsAuthorised = true;
            this.OperatorResponseCode = domainEvent.OperatorResponseCode;
            this.OperatorResponseMessage = domainEvent.OperatorResponseMessage;
            this.OperatorTransactionId = domainEvent.OperatorTransactionId;
            this.AuthorisationCode = domainEvent.AuthorisationCode;
            this.ResponseCode = domainEvent.ResponseCode;
            this.ResponseMessage = domainEvent.ResponseMessage;
        }

        private void PlayEvent(TransactionDeclinedByOperatorEvent domainEvent) {
            this.IsDeclined = true;
            this.OperatorResponseCode = domainEvent.OperatorResponseCode;
            this.OperatorResponseMessage = domainEvent.OperatorResponseMessage;
            this.ResponseCode = domainEvent.ResponseCode;
            this.ResponseMessage = domainEvent.ResponseMessage;
        }

        private void PlayEvent(ProductDetailsAddedToTransactionEvent domainEvent) {
            this.IsProductDetailsAdded = true;
            this.ContractId = domainEvent.ContractId;
            this.ProductId = domainEvent.ProductId;
        }

        private void PlayEvent(TransactionSourceAddedToTransactionEvent domainEvent) {
            this.TransactionSource = (TransactionSource)domainEvent.TransactionSource;
        }

        private void PlayEvent(MerchantFeeAddedToTransactionEvent domainEvent) {
            this.CalculatedFees.Add(new CalculatedFee {
                                                          CalculatedValue = domainEvent.CalculatedValue,
                                                          FeeId = domainEvent.FeeId,
                                                          FeeType = FeeType.Merchant,
                                                          FeeValue = domainEvent.FeeValue,
                                                          FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType
                                                      });
        }

        private void PlayEvent(ServiceProviderFeeAddedToTransactionEvent domainEvent) {
            this.CalculatedFees.Add(new CalculatedFee {
                                                          CalculatedValue = domainEvent.CalculatedValue,
                                                          FeeId = domainEvent.FeeId,
                                                          FeeType = FeeType.ServiceProvider,
                                                          FeeValue = domainEvent.FeeValue,
                                                          FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType
                                                      });
        }

        #endregion
    }
}