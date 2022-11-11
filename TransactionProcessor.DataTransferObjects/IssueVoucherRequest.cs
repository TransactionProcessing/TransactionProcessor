namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using Newtonsoft.Json;

    public class IssueVoucherRequest
    {
        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        [JsonProperty("operator_identifier")]
        public String OperatorIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty("transaction_id")]
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [JsonProperty("value")]
        public Decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the recipient email.
        /// </summary>
        /// <value>
        /// The recipient email.
        /// </value>
        [JsonProperty("recipient_email")]
        public String RecipientEmail { get; set; }

        /// <summary>
        /// Gets or sets the recipient mobile.
        /// </summary>
        /// <value>
        /// The recipient mobile.
        /// </value>
        [JsonProperty("recipient_mobile")]
        public String RecipientMobile { get; set; }

        /// <summary>
        /// Gets or sets the issued date time.
        /// </summary>
        /// <value>
        /// The issued date time.
        /// </value>
        [JsonProperty("issued_date_time")]
        public DateTime? IssuedDateTime { get; set; }
    }
}