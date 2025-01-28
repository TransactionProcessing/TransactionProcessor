using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    [ExcludeFromCodeCoverage]
    public class MakeMerchantWithdrawalRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [JsonProperty("amount")]
        public Decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the withdrawal date time.
        /// </summary>
        /// <value>
        /// The withdrawal date time.
        /// </value>
        [JsonProperty("withdrawal_date_time")]
        public DateTime WithdrawalDateTime { get; set; }

        /// <summary>
        /// Gets or sets the reference.
        /// </summary>
        /// <value>
        /// The reference.
        /// </value>
        [JsonProperty("reference")]
        public String Reference { get; set; }

        #endregion
    }
}