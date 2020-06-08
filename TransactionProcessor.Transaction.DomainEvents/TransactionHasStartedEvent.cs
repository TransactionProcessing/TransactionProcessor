namespace TransactionProcessor.Transaction.DomainEvents
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.EventSourcing.DomainEvent" />
    [JsonObject]
    public class TransactionHasStartedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHasStartedEvent" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TransactionHasStartedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHasStartedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        private TransactionHasStartedEvent(Guid aggregateId,
                                          Guid eventId,
                                          Guid estateId,
                                          Guid merchantId,
                                          DateTime transactionDateTime,
                                          String transactionNumber,
                                          String transactionType,
                                          String transactionReference,
                                          String deviceIdentifier,
                                          Decimal? transactionAmount) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.TransactionDateTime = transactionDateTime;
            this.TransactionNumber = transactionNumber;
            this.TransactionType = transactionType;
            this.TransactionReference = transactionReference;
            this.DeviceIdentifier = deviceIdentifier;
            this.TransactionAmount = transactionAmount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty]
        public Guid EstateId { get; private set; }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        [JsonProperty]
        public String DeviceIdentifier { get; private set; }

        /// <summary>
        /// Gets the transaction amount.
        /// </summary>
        /// <value>
        /// The transaction amount.
        /// </value>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Decimal? TransactionAmount { get; private set; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        [JsonProperty]
        public Guid MerchantId { get; private set; }

        /// <summary>
        /// Gets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        [JsonProperty]
        public DateTime TransactionDateTime { get; private set; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty]
        public Guid TransactionId { get; private set; }

        /// <summary>
        /// Gets the transaction number.
        /// </summary>
        /// <value>
        /// The transaction number.
        /// </value>
        [JsonProperty]
        public String TransactionNumber { get; private set; }

        /// <summary>
        /// Gets the type of the transaction.
        /// </summary>
        /// <value>
        /// The type of the transaction.
        /// </value>
        [JsonProperty]
        public String TransactionType { get; private set; }

        [JsonProperty]
        public String TransactionReference { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="transactionNumber">The transaction number.</param>
        /// <param name="transactionType">Type of the transaction.</param>
        /// <param name="transactionReference">The transaction reference.</param>
        /// <param name="deviceIdentifier">The device identifier.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        /// <returns></returns>
        public static TransactionHasStartedEvent Create(Guid aggregateId,
                                                        Guid estateId,
                                                        Guid merchantId,
                                                        DateTime transactionDateTime,
                                                        String transactionNumber,
                                                        String transactionType,
                                                        String transactionReference,
                                                        String deviceIdentifier,
                                                        Decimal? transactionAmount)
        {
            return new TransactionHasStartedEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, transactionDateTime, transactionNumber, transactionType, transactionReference, deviceIdentifier, transactionAmount);
        }

        #endregion
    }
}