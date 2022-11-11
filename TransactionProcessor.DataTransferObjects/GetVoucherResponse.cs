using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.DataTransferObjects
{
    public class GetVoucherResponse
    {
        [JsonProperty("voucher_id")]
        public Guid VoucherId { get; set; }

        /// <summary>
        /// Gets the expiry date.
        /// </summary>
        /// <value>
        /// The expiry date.
        /// </value>
        [JsonProperty("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [JsonProperty("value")]
        public Decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the balance.
        /// </summary>
        /// <value>
        /// The balance.
        /// </value>
        [JsonProperty("balance")]
        public Decimal Balance { get; set; }

        /// <summary>
        /// Gets the voucher code.
        /// </summary>
        /// <value>
        /// The voucher code.
        /// </value>
        [JsonProperty("voucher_code")]
        public String VoucherCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is generated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is generated; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("is_generated")]
        public Boolean IsGenerated { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is issued.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is issued; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("is_issued")]
        public Boolean IsIssued { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is redeemed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is redeemed; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("is_redeemed")]
        public Boolean IsRedeemed { get; set; }

        /// <summary>
        /// Gets or sets the issued date time.
        /// </summary>
        /// <value>
        /// The issued date time.
        /// </value>
        [JsonProperty("issued_date_time")]
        public DateTime IssuedDateTime { get; set; }
        /// <summary>
        /// Gets or sets the generated date time.
        /// </summary>
        /// <value>
        /// The generated date time.
        /// </value>
        [JsonProperty("generated_date_time")]
        public DateTime GeneratedDateTime { get; set; }
        /// <summary>
        /// Gets or sets the redeemed date time.
        /// </summary>
        /// <value>
        /// The redeemed date time.
        /// </value>
        [JsonProperty("redeemed_date_time")]
        public DateTime RedeemedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty("transaction_id")]
        public Guid TransactionId { get; set; }
    }
}
