namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using Newtonsoft.Json;

    public class IssueVoucherResponse
    {
        /// <summary>
        /// Gets or sets the voucher identifier.
        /// </summary>
        /// <value>
        /// The voucher identifier.
        /// </value>
        [JsonProperty("voucher_id")]
        public Guid VoucherId { get; set; }

        /// <summary>
        /// Gets or sets the expiry date.
        /// </summary>
        /// <value>
        /// The expiry date.
        /// </value>
        [JsonProperty("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the voucher code.
        /// </summary>
        /// <value>
        /// The voucher code.
        /// </value>
        [JsonProperty("voucher_code")]
        public String VoucherCode { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [JsonProperty("message")]
        public String Message { get; set; }
    }
}