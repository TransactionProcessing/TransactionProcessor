namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using Newtonsoft.Json;

    public class RedeemVoucherRequest
    {
        /// <summary>
        /// Gets or sets the voucher code.
        /// </summary>
        /// <value>
        /// The voucher code.
        /// </value>
        [JsonProperty("voucher_code")]
        public String VoucherCode { get; set; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        [JsonProperty("redeemed_date_time")]
        public DateTime? RedeemedDateTime { get; set; }
    }
}