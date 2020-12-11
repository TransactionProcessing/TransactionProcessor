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
    public class TransactionHasBeenCompletedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHasBeenCompletedEvent"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public TransactionHasBeenCompletedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHasBeenCompletedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="isAuthorised">if set to <c>true</c> [is authorised].</param>
        /// <param name="completedDateTime">The completed date time.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        private TransactionHasBeenCompletedEvent(Guid aggregateId,
                                                Guid eventId,
                                                Guid estateId,
                                                Guid merchantId,
                                                String responseCode,
                                                String responseMessage,
                                                Boolean isAuthorised,
                                                DateTime completedDateTime,
                                                Decimal? transactionAmount) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.ResponseCode = responseCode;
            this.ResponseMessage = responseMessage;
            this.IsAuthorised = isAuthorised;
            this.CompletedDateTime = completedDateTime;
            this.TransactionAmount = transactionAmount;
        }

        #endregion

        #region Properties

        [JsonProperty]
        public DateTime CompletedDateTime { get; private set; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty]
        public Guid EstateId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is authorised.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authorised; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty]
        public Boolean IsAuthorised { get; private set; }

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
        /// Gets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [JsonProperty]
        public String ResponseCode { get; private set; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        [JsonProperty]
        public String ResponseMessage { get; private set; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty]
        public Guid TransactionId { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="isAuthorised">if set to <c>true</c> [is authorised].</param>
        /// <param name="completedDateTime">The completed date time.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        /// <returns></returns>
        public static TransactionHasBeenCompletedEvent Create(Guid aggregateId,
                                                              Guid estateId,
                                                              Guid merchantId,
                                                              String responseCode,
                                                              String responseMessage,
                                                              Boolean isAuthorised,
                                                              DateTime completedDateTime,
                                                              Decimal? transactionAmount)
        {
            return new TransactionHasBeenCompletedEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, responseCode, responseMessage, isAuthorised, completedDateTime, transactionAmount);
        }

        #endregion
    }
}