using System;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    public class GenerateMerchantStatementRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the statement date.
        /// </summary>
        /// <value>
        /// The statement date.
        /// </value>
        [JsonProperty("merchant_statement_date")]
        public DateTime MerchantStatementDate { get; set; }

        #endregion
    }
}