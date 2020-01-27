namespace TransactionProcessor.TransactionAggregate
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.RegularExpressions;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.DomainDrivenDesign.EventStore;
    using Shared.General;
    using Transaction.DomainEvents;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.EventStore.Aggregate" />
    public class TransactionAggregate : Aggregate
    {
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
        /// Gets a value indicating whether this instance is authorised.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authorised; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsAuthorised { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is declined.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is declined; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsDeclined { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is completed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is completed; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsCompleted { get; private set; }

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
        /// Gets the type of the transaction.
        /// </summary>
        /// <value>
        /// The type of the transaction.
        /// </value>
        public String TransactionType { get; private set; }

        #endregion

        #region Methods

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
                TransactionHasBeenCompletedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId, this.ResponseCode, this.ResponseMessage, true);

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
        /// Checks the transaction not already declined.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction [{this.AggregateId}] has already been{authtype}declined</exception>
        private void CheckTransactionNotAlreadyDeclined()
        {
            if (this.IsLocallyDeclined || this.IsDeclined)
            {
                String authtype = this.IsLocallyAuthorised ? " locally " : " ";
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has already been{authtype}declined");
            }
        }

        /// <summary>
        /// Starts the transaction.
        /// </summary>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <exception cref="ArgumentException">
        /// Transaction Number must be numeric
        /// or
        /// Invalid Transaction Type [{transactionType}]
        /// or
        /// Device Identifier must be alphanumeric
        /// </exception>
        public void StartTransaction(DateTime transactionDateTime,
                                     String transactionNumber,
                                     String transactionType,
                                     Guid estateId,
                                     Guid merchantId,
                                     String deviceIdentifier)
        {
            Guard.ThrowIfInvalidDate(transactionDateTime, typeof(ArgumentException), $"Transaction Date Time must not be [{DateTime.MinValue}]");
            Guard.ThrowIfNullOrEmpty(transactionNumber, typeof(ArgumentException), "Transaction Number must not be null or empty");
            if (int.TryParse(transactionNumber, out Int32 txnnumber) == false)
            {
                throw new ArgumentException("Transaction Number must be numeric");
            }

            Guard.ThrowIfNullOrEmpty(transactionType, typeof(ArgumentException), "Transaction Type must not be null or empty");

            // Temporary validation until using enum
            if (transactionType != "Logon")
            {
                throw new ArgumentException($"Invalid Transaction Type [{transactionType}]");
            }

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
            TransactionHasStartedEvent transactionHasStartedEvent =
                TransactionHasStartedEvent.Create(this.AggregateId, estateId, merchantId, transactionDateTime, transactionNumber, transactionType, deviceIdentifier);

            this.ApplyAndPend(transactionHasStartedEvent);
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

        /// <summary>
        /// Checks the transaction has been authorised.
        /// </summary>
        /// <exception cref="InvalidOperationException">Transaction [{this.AggregateId}] has not been authorised</exception>
        private void CheckTransactionHasBeenAuthorisedOrDeclined()
        {
            if (this.IsAuthorised == false && this.IsLocallyAuthorised == false &&
                this.IsDeclined == false && this.IsLocallyDeclined == false)
            {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been authorised or declined");
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
        private void PlayEvent(TransactionHasStartedEvent domainEvent)
        {
            this.MerchantId = domainEvent.MerchantId;
            this.EstateId = domainEvent.EstateId;
            this.DeviceIdentifier = domainEvent.DeviceIdentifier;
            this.IsStarted = true;
            this.TransactionDateTime = domainEvent.TransactionDateTime;
            this.TransactionNumber = domainEvent.TransactionNumber;
            this.TransactionType = domainEvent.TransactionType;
            this.IsLocallyDeclined = false;
            this.IsDeclined = false;
            this.IsLocallyAuthorised = false;
            this.IsAuthorised = false;
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

        #endregion
    }
}