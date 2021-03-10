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
    public record TransactionHasStartedEvent : DomainEventRecord.DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHasStartedEvent" /> class.
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
        public TransactionHasStartedEvent(Guid aggregateId,
                                          Guid estateId,
                                          Guid merchantId,
                                          DateTime transactionDateTime,
                                          String transactionNumber,
                                          String transactionType,
                                          String transactionReference,
                                          String deviceIdentifier,
                                          Decimal? transactionAmount) : base(aggregateId, Guid.NewGuid())
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
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        public String DeviceIdentifier { get; init; }

        /// <summary>
        /// Gets the transaction amount.
        /// </summary>
        /// <value>
        /// The transaction amount.
        /// </value>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Decimal? TransactionAmount { get; init; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; init; }

        /// <summary>
        /// Gets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        public DateTime TransactionDateTime { get; init; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public Guid TransactionId { get; init; }

        /// <summary>
        /// Gets the transaction number.
        /// </summary>
        /// <value>
        /// The transaction number.
        /// </value>
        public String TransactionNumber { get; init; }

        /// <summary>
        /// Gets the type of the transaction.
        /// </summary>
        /// <value>
        /// The type of the transaction.
        /// </value>
        public String TransactionType { get; init; }
        
        /// <summary>
        /// Gets or sets the transaction reference.
        /// </summary>
        /// <value>
        /// The transaction reference.
        /// </value>
        public String TransactionReference { get; init; }

        #endregion
    }
}