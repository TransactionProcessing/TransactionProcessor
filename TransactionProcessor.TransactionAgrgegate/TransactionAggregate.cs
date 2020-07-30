namespace TransactionProcessor.TransactionAggregate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
    using Models;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.EventStore;
    using Shared.General;
    using Transaction.DomainEvents;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Aggregate" />
    public class TransactionAggregate : Aggregate
    {
        #region Fields

        /// <summary>
        /// The additional transaction request metadata
        /// </summary>
        private Dictionary<String, String> AdditionalTransactionRequestMetadata;

        /// <summary>
        /// The additional transaction response metadata
        /// </summary>
        private Dictionary<String, String> AdditionalTransactionResponseMetadata;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAggregate" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TransactionAggregate()
        {
            // Nothing here
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAggregate" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private TransactionAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the authorisation code.
        /// </summary>
        /// <value>
        /// The authorisation code.
        /// </value>
        public String AuthorisationCode { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [customer email receipt has been requested].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [customer email receipt has been requested]; otherwise, <c>false</c>.
        /// </value>
        public Boolean CustomerEmailReceiptHasBeenRequested { get; private set; }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        public String DeviceIdentifier { get; private set; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; private set; }

        /// <summary>
        /// Gets the contract identifier.
        /// </summary>
        /// <value>
        /// The contract identifier.
        /// </value>
        public Guid ContractId { get; private set; }
        /// <summary>
        /// Gets the product identifier.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is authorised.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authorised; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsAuthorised { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is completed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is completed; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsCompleted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is declined.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is declined; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsDeclined { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is locally authorised.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is locally authorised; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsLocallyAuthorised { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is locally declined.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is locally declined; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsLocallyDeclined { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is started; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsStarted { get; private set; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; private set; }

        /// <summary>
        /// Gets the operator response code.
        /// </summary>
        /// <value>
        /// The operator response code.
        /// </value>
        public String OperatorResponseCode { get; private set; }

        /// <summary>
        /// Gets the operator response message.
        /// </summary>
        /// <value>
        /// The operator response message.
        /// </value>
        public String OperatorResponseMessage { get; private set; }

        /// <summary>
        /// Gets the operator transaction identifier.
        /// </summary>
        /// <value>
        /// The operator transaction identifier.
        /// </value>
        public String OperatorTransactionId { get; private set; }

        /// <summary>
        /// Gets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public String ResponseCode { get; private set; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; private set; }

        /// <summary>
        /// Gets the transaction amount.
        /// </summary>
        /// <value>
        /// The transaction amount.
        /// </value>
        public Decimal? TransactionAmount { get; private set; }

        /// <summary>
        /// Gets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        public DateTime TransactionDateTime { get; private set; }

        /// <summary>
        /// Gets the transaction number.
        /// </summary>
        /// <value>
        /// The transaction number.
        /// </value>
        public String TransactionNumber { get; private set; }

        /// <summary>
        /// Gets the transaction reference.
        /// </summary>
        /// <value>
        /// The transaction reference.
        /// </value>
        public String TransactionReference { get; private set; }

        /// <summary>
        /// Gets the type of the transaction.
        /// </summary>
        /// <value>
        /// The type of the transaction.
        /// </value>
        public TransactionType TransactionType { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Authorises the transaction.
        /// </summary>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="authorisationCode">The authorisation code.</param>
        /// <param name="operatorResponseCode">The operator response code.</param>
        /// <param name="operatorResponseMessage">The operator response message.</param>
        /// <param name="operatorTransactionId">The operator transaction identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public void AuthoriseTransaction(String operatorIdentifier,
                                         String authorisationCode,
                                         String operatorResponseCode,
                                         String operatorResponseMessage,
                                         String operatorTransactionId,
                                         String responseCode,
                                         String responseMessage)
        {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();

            TransactionAuthorisedByOperatorEvent transactionAuthorisedByOperatorEvent = TransactionAuthorisedByOperatorEvent.Create(this.AggregateId,
                                                                                                                                    this.EstateId,
                                                                                                                                    this.MerchantId,
                                                                                                                                    operatorIdentifier,
                                                                                                                                    authorisationCode,
                                                                                                                                    operatorResponseCode,
                                                                                                                                    operatorResponseMessage,
                                                                                                                                    operatorTransactionId,
                                                                                                                                    responseCode,
                                                                                                                                    responseMessage);
            this.ApplyAndPend(transactionAuthorisedByOperatorEvent);
        }

        /// <summary>
        /// Authorises the transaction.
        /// </summary>
        /// <param name="authorisationCode">The authorisation code.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public void AuthoriseTransactionLocally(String authorisationCode,
                                                String responseCode,
                                                String responseMessage)
        {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();
            this.CheckTransactionCanBeLocallyAuthorised();
            TransactionHasBeenLocallyAuthorisedEvent transactionHasBeenLocallyAuthorisedEvent =
                TransactionHasBeenLocallyAuthorisedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId, authorisationCode, responseCode, responseMessage);

            this.ApplyAndPend(transactionHasBeenLocallyAuthorisedEvent);
        }

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        public void CompleteTransaction()
        {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionHasBeenAuthorisedOrDeclined();
            this.CheckTransactionNotAlreadyCompleted();

            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent =
                TransactionHasBeenCompletedEvent.Create(this.AggregateId,
                                                        this.EstateId,
                                                        this.MerchantId,
                                                        this.ResponseCode,
                                                        this.ResponseMessage,
                                                        this.IsAuthorised || this.IsLocallyAuthorised);

            this.ApplyAndPend(transactionHasBeenCompletedEvent);
        }

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public static TransactionAggregate Create(Guid aggregateId)
        {
            return new TransactionAggregate(aggregateId);
        }

        /// <summary>
        /// Declines the transaction.
        /// </summary>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="operatorResponseCode">The operator response code.</param>
        /// <param name="operatorResponseMessage">The operator response message.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public void DeclineTransaction(String operatorIdentifier,
                                       String operatorResponseCode,
                                       String operatorResponseMessage,
                                       String responseCode,
                                       String responseMessage)
        {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();
            this.CheckTransactionNotAlreadyDeclined();

            TransactionDeclinedByOperatorEvent transactionDeclinedByOperatorEvent =
                TransactionDeclinedByOperatorEvent.Create(this.AggregateId,
                                                          this.EstateId,
                                                          this.MerchantId,
                                                          operatorIdentifier,
                                                          operatorResponseCode,
                                                          operatorResponseMessage,
                                                          responseCode,
                                                          responseMessage);
            this.ApplyAndPend(transactionDeclinedByOperatorEvent);
        }

        /// <summary>
        /// Declines the transaction.
        /// </summary>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public void DeclineTransactionLocally(String responseCode,
                                              String responseMessage)
        {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();
            this.CheckTransactionNotAlreadyDeclined();
            TransactionHasBeenLocallyDeclinedEvent transactionHasBeenLocallyDeclinedEvent =
                TransactionHasBeenLocallyDeclinedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId, responseCode, responseMessage);

            this.ApplyAndPend(transactionHasBeenLocallyDeclinedEvent);
        }

        /// <summary>
        /// Records the additional request data.
        /// </summary>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="additionalTransactionRequestMetadata">The additional transaction request metadata.</param>
        public void RecordAdditionalRequestData(String operatorIdentifier,
                                                Dictionary<String, String> additionalTransactionRequestMetadata)
        {
            this.CheckTransactionNotAlreadyCompleted();
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyAuthorised();
            this.CheckTransactionNotAlreadyDeclined();
            this.CheckAdditionalRequestDataNotAlreadyRecorded();

            AdditionalRequestDataRecordedEvent additionalRequestDataRecordedEvent =
                AdditionalRequestDataRecordedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId, operatorIdentifier, additionalTransactionRequestMetadata);

            this.ApplyAndPend(additionalRequestDataRecordedEvent);
        }

        /// <summary>
        /// Records the additional response data.
        /// </summary>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="additionalTransactionResponseMetadata">The additional transaction response metadata.</param>
        public void RecordAdditionalResponseData(String operatorIdentifier,
                                                 Dictionary<String, String> additionalTransactionResponseMetadata)
        {
            this.CheckTransactionHasBeenStarted();
            this.CheckAdditionalResponseDataNotAlreadyRecorded();

            AdditionalResponseDataRecordedEvent additionalResponseDataRecordedEvent =
                AdditionalResponseDataRecordedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId, operatorIdentifier, additionalTransactionResponseMetadata);

            this.ApplyAndPend(additionalResponseDataRecordedEvent);
        }

        /// <summary>
        /// Requests the email receipt.
        /// </summary>
        /// <param name="customerEmailAddress">The customer email address.</param>
        public void RequestEmailReceipt(String customerEmailAddress)
        {
            this.CheckTransactionHasBeenCompleted();
            this.CheckCustomerHasNotAlreadyRequestedEmailReceipt();

            CustomerEmailReceiptRequestedEvent customerEmailReceiptRequestedEvent =
                CustomerEmailReceiptRequestedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId, customerEmailAddress);

            this.ApplyAndPend(customerEmailReceiptRequestedEvent);
        }

        /// <summary>
        /// Starts the transaction.
        /// </summary>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        /// <exception cref="ArgumentException">Transaction Number must be numeric
        /// or
        /// Invalid Transaction Type [{transactionType}]
        /// or
        /// Device Identifier must be alphanumeric</exception>
        public void StartTransaction(DateTime transactionDateTime,
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
            if (int.TryParse(transactionNumber, out Int32 txnnumber) == false)
            {
                throw new ArgumentException("Transaction Number must be numeric");
            }

            // Validate the transaction Type
            Guard.ThrowIfInvalidEnum(typeof(TransactionType), transactionType, typeof(ArgumentOutOfRangeException), $"Invalid Transaction Type [{transactionType}]");

            Guard.ThrowIfInvalidGuid(estateId, typeof(ArgumentException), $"Estate Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(merchantId, typeof(ArgumentException), $"Merchant Id must not be [{Guid.Empty}]");
            Guard.ThrowIfNullOrEmpty(deviceIdentifier, typeof(ArgumentException), "Device Identifier must not be null or empty");

            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (r.IsMatch(deviceIdentifier) == false)
            {
                throw new ArgumentException("Device Identifier must be alphanumeric");
            }

            this.CheckTransactionNotAlreadyStarted();
            this.CheckTransactionNotAlreadyCompleted();
            TransactionHasStartedEvent transactionHasStartedEvent = TransactionHasStartedEvent.Create(this.AggregateId,
                                                                                                      estateId,
                                                                                                      merchantId,
                                                                                                      transactionDateTime,
                                                                                                      transactionNumber,
                                                                                                      transactionType.ToString(),
                                                                                                      transactionReference,
                                                                                                      deviceIdentifier,
                                                                                                      transactionAmount);

            this.ApplyAndPend(transactionHasStartedEvent);
        }

        /// <summary>
        /// Adds the product details.
        /// </summary>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="productId">The product identifier.</param>
        public void AddProductDetails(Guid contractId,
                                      Guid productId)
        {
            Guard.ThrowIfInvalidGuid(contractId, typeof(ArgumentException), $"Contract Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(productId, typeof(ArgumentException), $"Product Id must not be [{Guid.Empty}]");

            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionNotAlreadyCompleted();
            this.CheckProductDetailsNotAlreadyAdded();

            ProductDetailsAddedToTransactionEvent productDetailsAddedToTransactionEvent = ProductDetailsAddedToTransactionEvent.Create(this.AggregateId,
                                                                                                                                       this.EstateId,
                                                                                                                                       contractId,
                                                                                                                                       productId);

            this.ApplyAndPend(productDetailsAddedToTransactionEvent);
        }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
                   {
                       this.EstateId
                   };
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        protected override void PlayEvent(DomainEvent domainEvent)
        {
            this.PlayEvent((dynamic)domainEvent);
        }

        private void CheckProductDetailsNotAlreadyAdded()
        {
            if (this.IsProductDetailsAdded)
            {
                throw new InvalidOperationException("Product details already added");
            }
        }

        /// <summary>
        /// Checks the additional request data not already recorded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Additional Request Data already recorded</exception>
        private void CheckAdditionalRequestDataNotAlreadyRecorded()
        {
            if (this.AdditionalTransactionRequestMetadata != null)
            {
                throw new InvalidOperationException("Additional Request Data already recorded");
            }
        }

        /// <summary>
        /// Checks the additional response data not already recorded.
        /// </summary>
        /// <exception cref="InvalidOperationException">Additional Response Data already recorded</exception>
        private void CheckAdditionalResponseDataNotAlreadyRecorded()
        {
            if (this.AdditionalTransactionResponseMetadata != null)
            {
                throw new InvalidOperationException("Additional Response Data already recorded");
            }
        }

        /// <summary>
        /// Checks the customer has not already requested email receipt.
        /// </summary>
        /// <exception cref="InvalidOperationException">Customer Email Receipt already requested for Transaction [{this.AggregateId}]</exception>
        private void CheckCustomerHasNotAlreadyRequestedEmailReceipt()
        {
            if (this.CustomerEmailReceiptHasBeenRequested)
            {
                throw new InvalidOperationException($"Customer Email Receipt already requested for Transaction [{this.AggregateId}]");
            }
        }

        /// <summary>
        /// Checks the transaction can be locally authorised.
        /// </summary>
        /// <exception cref="InvalidOperationException">Sales cannot be locally authorised</exception>
        /// <exception cref="NotSupportedException">Sales cannot be locally authorised</exception>
        private void CheckTransactionCanBeLocallyAuthorised()
        {
            if (this.TransactionType == TransactionType.Sale)
            {
                throw new InvalidOperationException("Sales cannot be locally authorised");
            }
        }

        /// <summary>
        /// Checks the transaction has been authorised.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction [{this.AggregateId}] has not been authorised</exception>
        private void CheckTransactionHasBeenAuthorisedOrDeclined()
        {
            if (this.IsAuthorised == false && this.IsLocallyAuthorised == false && this.IsDeclined == false && this.IsLocallyDeclined == false)
            {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been authorised or declined");
            }
        }

        /// <summary>
        /// Checks the transaction has been completed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction [{this.AggregateId}] has not been completed</exception>
        private void CheckTransactionHasBeenCompleted()
        {
            if (this.IsCompleted == false)
            {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been completed");
            }
        }

        /// <summary>
        /// Checks the transaction has been started.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction [{this.AggregateId}] has not been started</exception>
        private void CheckTransactionHasBeenStarted()
        {
            if (this.IsStarted == false)
            {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been started");
            }
        }

        /// <summary>
        /// Checks the transaction not already authorised.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction [{this.AggregateId}] has already been{authtype}authorised</exception>
        private void CheckTransactionNotAlreadyAuthorised()
        {
            if (this.IsLocallyAuthorised || this.IsAuthorised)
            {
                String authtype = this.IsLocallyAuthorised ? " locally " : " ";
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has already been{authtype}authorised");
            }
        }

        /// <summary>
        /// Checks the transaction not already completed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction Id [{this.AggregateId}] has already been completed</exception>
        private void CheckTransactionNotAlreadyCompleted()
        {
            if (this.IsCompleted)
            {
                throw new InvalidOperationException($"Transaction Id [{this.AggregateId}] has already been completed");
            }
        }

        /// <summary>
        /// Checks the transaction not already declined.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction [{this.AggregateId}] has already been{authtype}declined</exception>
        private void CheckTransactionNotAlreadyDeclined()
        {
            if (this.IsLocallyDeclined || this.IsDeclined)
            {
                String authtype = this.IsLocallyDeclined ? " locally " : " ";
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has already been{authtype}declined");
            }
        }

        /// <summary>
        /// Checks the transaction not already started.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction Id [{this.AggregateId}] has already been started</exception>
        private void CheckTransactionNotAlreadyStarted()
        {
            if (this.IsStarted)
            {
                throw new InvalidOperationException($"Transaction Id [{this.AggregateId}] has already been started");
            }
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(CustomerEmailReceiptRequestedEvent domainEvent)
        {
            this.CustomerEmailReceiptHasBeenRequested = true;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(AdditionalRequestDataRecordedEvent domainEvent)
        {
            this.AdditionalTransactionRequestMetadata = domainEvent.AdditionalTransactionRequestMetadata;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(AdditionalResponseDataRecordedEvent domainEvent)
        {
            this.AdditionalTransactionResponseMetadata = domainEvent.AdditionalTransactionResponseMetadata;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(TransactionHasStartedEvent domainEvent)
        {
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

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(TransactionHasBeenLocallyAuthorisedEvent domainEvent)
        {
            this.IsLocallyAuthorised = true;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.ResponseCode = domainEvent.ResponseCode;
            this.AuthorisationCode = domainEvent.AuthorisationCode;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(TransactionHasBeenLocallyDeclinedEvent domainEvent)
        {
            this.IsLocallyDeclined = true;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.ResponseCode = domainEvent.ResponseCode;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(TransactionHasBeenCompletedEvent domainEvent)
        {
            this.IsStarted = false; // Transaction has reached its final state
            this.IsCompleted = true;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(TransactionAuthorisedByOperatorEvent domainEvent)
        {
            this.IsAuthorised = true;
            this.OperatorResponseCode = domainEvent.OperatorResponseCode;
            this.OperatorResponseMessage = domainEvent.OperatorResponseMessage;
            this.OperatorTransactionId = domainEvent.OperatorTransactionId;
            this.AuthorisationCode = domainEvent.AuthorisationCode;
            this.ResponseCode = domainEvent.ResponseCode;
            this.ResponseMessage = domainEvent.ResponseMessage;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(TransactionDeclinedByOperatorEvent domainEvent)
        {
            this.IsDeclined = true;
            this.OperatorResponseCode = domainEvent.OperatorResponseCode;
            this.OperatorResponseMessage = domainEvent.OperatorResponseMessage;
            this.ResponseCode = domainEvent.ResponseCode;
            this.ResponseMessage = domainEvent.ResponseMessage;
        }

        private void PlayEvent(ProductDetailsAddedToTransactionEvent domainEvent)
        {
            this.IsProductDetailsAdded = true;
            this.ContractId = domainEvent.ContractId;
            this.ProductId = domainEvent.ProductId;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is product details added.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is product details added; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsProductDetailsAdded { get; private set; }

        #endregion
    }
}