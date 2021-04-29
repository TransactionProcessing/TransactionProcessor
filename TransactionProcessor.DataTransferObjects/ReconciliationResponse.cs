namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class ReconciliationResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [JsonProperty("response_code")]
        public String ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        [JsonProperty("response_message")]
        public String ResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty("transaction_id")]
        public Guid TransactionId { get; set; }

        #endregion
    }
}