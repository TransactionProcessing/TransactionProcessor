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
        /// <param name="imeiNumber">The imei number.</param>
        public TransactionHasStartedEvent(Guid aggregateId,
                                          Guid eventId,
                                          Guid estateId,
                                          Guid merchantId,
                                          DateTime transactionDateTime,
                                          String transactionNumber,
                                          String transactionType,
                                          String imeiNumber) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.TransactionDateTime = transactionDateTime;
            this.TransactionNumber = transactionNumber;
            this.TransactionType = transactionType;
            this.ImeiNumber = imeiNumber;
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
        /// Gets the imei number.
        /// </summary>
        /// <value>
        /// The imei number.
        /// </value>
        [JsonProperty]
        public String ImeiNumber { get; private set; }

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
        /// <param name="imeiNumber">The imei number.</param>
        /// <returns></returns>
        public static TransactionHasStartedEvent Create(Guid aggregateId,
                                                        Guid estateId,
                                                        Guid merchantId,
                                                        DateTime transactionDateTime,
                                                        String transactionNumber,
                                                        String transactionType,
                                                        String imeiNumber)
        {
            return new TransactionHasStartedEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, transactionDateTime, transactionNumber, transactionType, imeiNumber);
        }

        #endregion
    }
}