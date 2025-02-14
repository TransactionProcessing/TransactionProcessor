namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using Newtonsoft.Json;

    public class RedeemVoucherResponse
    {
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
        /// Gets or sets the remaining balance.
        /// </summary>
        /// <value>
        /// The remaining balance.
        /// </value>
        [JsonProperty("remaining_balance")]
        public Decimal RemainingBalance { get; set; }
    }
}