namespace TransactionProcessor.Voucher.DomainEvents
{
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record VoucherIssuedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VoucherIssuedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="issuedDateTime">The issued date time.</param>
        /// <param name="recipientEmail">The recipient email.</param>
        /// <param name="recipientMobile">The recipient mobile.</param>
        public VoucherIssuedEvent(Guid aggregateId,
                                  Guid estateId,
                                  DateTime issuedDateTime,
                                  String recipientEmail,
                                  String recipientMobile) : base(aggregateId, Guid.NewGuid())
        {
            this.EstateId = estateId;
            this.VoucherId = aggregateId;
            this.IssuedDateTime = issuedDateTime;
            this.RecipientEmail = recipientEmail;
            this.RecipientMobile = recipientMobile;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets or sets the issued date time.
        /// </summary>
        /// <value>
        /// The issued date time.
        /// </value>
        public DateTime IssuedDateTime { get; init; }

        /// <summary>
        /// Gets or sets the recipient email.
        /// </summary>
        /// <value>
        /// The recipient email.
        /// </value>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public String RecipientEmail { get; init; }

        /// <summary>
        /// Gets or sets the recipient mobile.
        /// </summary>
        /// <value>
        /// The recipient mobile.
        /// </value>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public String RecipientMobile { get; init; }

        /// <summary>
        /// Gets or sets the voucher identifier.
        /// </summary>
        /// <value>
        /// The voucher identifier.
        /// </value>
        public Guid VoucherId { get; init; }

        #endregion
    }
}