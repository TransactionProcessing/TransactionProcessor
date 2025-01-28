using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    [ExcludeFromCodeCoverage]
    public class EstateOperatorResponse
    {
        #region Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        [JsonProperty("operator_id")]
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Gets a value indicating whether [require custom merchant number].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require custom merchant number]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("require_custom_merchant_number")]
        public Boolean RequireCustomMerchantNumber { get; set; }

        /// <summary>
        /// Gets a value indicating whether [require customterminal number].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require customterminal number]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("require_custom_terminal_number")]
        public Boolean RequireCustomTerminalNumber { get; set; }

        [JsonProperty("is_deleted")]
        public Boolean IsDeleted { get; set; }

        #endregion
    }
}