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
    public record TransactionHasBeenCompletedEvent : DomainEventRecord.DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHasBeenCompletedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="isAuthorised">if set to <c>true</c> [is authorised].</param>
        /// <param name="completedDateTime">The completed date time.</param>
        /// <param name="transactionAmount">The transaction amount.</param>
        public TransactionHasBeenCompletedEvent(Guid aggregateId,
                                                Guid estateId,
                                                Guid merchantId,
                                                String responseCode,
                                                String responseMessage,
                                                Boolean isAuthorised,
                                                DateTime completedDateTime,
                                                Decimal? transactionAmount) : base(aggregateId, Guid.NewGuid())
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

        /// <summary>
        /// Gets or sets the completed date time.
        /// </summary>
        /// <value>
        /// The completed date time.
        /// </value>
        public DateTime CompletedDateTime { get; init; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets a value indicating whether this instance is authorised.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authorised; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsAuthorised { get; init; }

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
        /// Gets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public String ResponseCode { get; init; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; init; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public Guid TransactionId { get; init; }

        #endregion
    }
}