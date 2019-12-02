namespace TransactionProcessor.TransactionAggregate
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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
            TransactionHasBeenLocallyAuthorisedEvent transactionHasBeenLocallyAuthorisedEvent = TransactionHasBeenLocallyAuthorisedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId, authorisationCode, responseCode, responseMessage);

            this.ApplyAndPend(transactionHasBeenLocallyAuthorisedEvent);


        }

        private void CheckTransactionNotAlreadyAuthorised()
        {
            if (this.IsLocallyAuthorised || this.IsAuthorised)
            {
                String authtype = this.IsLocallyAuthorised ? " locally " : " ";
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has already been{authtype}authorised");
            }
        }

        private void CheckTransactionHasBeenStarted()
        {
            if (this.IsStarted == false)
            {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been started");
            }
        }

        /// <summary>
        /// Completes the transaction.
        /// </summary>
        public void CompleteTransaction()
        {
            this.CheckTransactionHasBeenStarted();
            this.CheckTransactionHasBeenAuthorised();
            this.CheckTransactionNotAlreadyCompleted();

            TransactionHasBeenCompletedEvent transactionHasBeenCompletedEvent = TransactionHasBeenCompletedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId,
                                                                                                                        this.ResponseCode, this.ResponseMessage,
                                                                                                                        true);

            this.ApplyAndPend(transactionHasBeenCompletedEvent);
        }

        private void CheckTransactionHasBeenAuthorised()
        {
            if (this.IsAuthorised == false && this.IsLocallyAuthorised == false)
            {
                throw new InvalidOperationException($"Transaction [{this.AggregateId}] has not been authorised");
            }
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
        public void DeclineTransaction()
        {
        }

        public DateTime TransactionDateTime { get; private set; }
        public String TransactionNumber { get; private set; }
        public String TransactionType { get; private set; }
        public Guid EstateId { get; private set; }
        public Guid MerchantId { get; private set; }
        public String IMEINumber { get; private set; }
        public Boolean IsStarted { get; private set; }
        public Boolean IsCompleted { get; private set; }

        public Boolean IsLocallyAuthorised { get; private set; }
        public String AuthorisationCode { get; private set; }
        public String ResponseCode { get; private set; }
        public String ResponseMessage { get; private set; }
        public Boolean IsAuthorised { get; private set; }
        /// <summary>
        /// Starts the transaction.
        /// </summary>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="imeiNumber">The imei number.</param>
        public void StartTransaction(DateTime transactionDateTime,
                                     String transactionNumber,
                                     String transactionType,
                                     Guid estateId,
                                     Guid merchantId,
                                     String imeiNumber)
        {
            Guard.ThrowIfInvalidDate(transactionDateTime, typeof(ArgumentException), $"Transaction Date Time must not be [{DateTime.MinValue}]");
            Guard.ThrowIfNullOrEmpty(transactionNumber, typeof(ArgumentException), "Transaction Number must not be null or empty");
            if (Int32.TryParse(transactionNumber, out Int32 txnnumber) == false)
            {
                throw new ArgumentException("Transaction Number must be numeric");
            }
            Guard.ThrowIfNullOrEmpty(transactionType, typeof(ArgumentException), "Transaction Type must not be null or empty");

            // Temporary validation until using enum
            if (transactionType != "Logon")
            {
                throw  new ArgumentException($"Invalid Transaction Type [{transactionType}]");
            }
            Guard.ThrowIfInvalidGuid(estateId, typeof(ArgumentException), $"Estate Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(merchantId, typeof(ArgumentException), $"Merchant Id must not be [{Guid.Empty}]");
            Guard.ThrowIfNullOrEmpty(imeiNumber, typeof(ArgumentException), "IMEI Number must not be null or empty");
            if (Int32.TryParse(imeiNumber, out Int32 imei) == false)
            {
                throw new ArgumentException("IMEI Number must be numeric");
            }

            this.CheckTransactionNotAlreadyStarted();
            this.CheckTransactionNotAlreadyCompleted();
            TransactionHasStartedEvent transactionHasStartedEvent = TransactionHasStartedEvent.Create(this.AggregateId, estateId, merchantId, transactionDateTime, transactionNumber,transactionType, imeiNumber);

            this.ApplyAndPend(transactionHasStartedEvent);
        }

        private void CheckTransactionNotAlreadyStarted()
        {
            if (this.IsStarted)
            {
                throw new InvalidOperationException($"Transaction Id [{this.AggregateId}] has already been started");
            }
        }

        private void CheckTransactionNotAlreadyCompleted()
        {
            if (this.IsCompleted)
            {
                throw new InvalidOperationException($"Transaction Id [{this.AggregateId}] has already been completed");
            }
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
                       EstateId = this.EstateId
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

        private void PlayEvent(TransactionHasStartedEvent domainEvent)
        {
            this.MerchantId = domainEvent.MerchantId;
            this.EstateId = domainEvent.EstateId;
            this.IMEINumber = domainEvent.ImeiNumber;
            this.IsStarted = true;
            this.TransactionDateTime = domainEvent.TransactionDateTime;
            this.TransactionNumber = domainEvent.TransactionNumber;
            this.TransactionType = domainEvent.TransactionType;
        }

        private void PlayEvent(TransactionHasBeenLocallyAuthorisedEvent domainEvent)
        {
            this.IsLocallyAuthorised = true;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.ResponseCode = domainEvent.ResponseCode;
            this.AuthorisationCode = domainEvent.AuthorisationCode;
        }

        private void PlayEvent(TransactionHasBeenCompletedEvent domainEvent)
        {
            this.IsStarted = false; // Transaction has reached its final state
            this.IsCompleted = true;
        }

        #endregion
    }
}