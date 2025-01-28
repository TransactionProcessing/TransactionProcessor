using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AssignOperatorRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the merchant number.
        /// </summary>
        /// <value>
        /// The merchant number.
        /// </value>
        [JsonProperty("merchant_number")]
        public String MerchantNumber { get; set; }

        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        [JsonProperty("operator_id")]
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Gets or sets the terminal number.
        /// </summary>
        /// <value>
        /// The terminal number.
        /// </value>
        [JsonProperty("terminal_number")]
        public String TerminalNumber { get; set; }

        #endregion
    }
}