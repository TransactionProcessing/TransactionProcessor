using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    [ExcludeFromCodeCoverage]
    public class MakeMerchantDepositResponse
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
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        [JsonProperty("deposit_id")]
        public Guid DepositId { get; set; }

        #endregion
    }
}
