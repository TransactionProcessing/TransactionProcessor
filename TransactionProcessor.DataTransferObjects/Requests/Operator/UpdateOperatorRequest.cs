using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Operator{
    [ExcludeFromCodeCoverage]
    public class UpdateOperatorRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [require custom merchant number].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require custom merchant number]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [JsonProperty("require_custom_merchant_number")]
        public Boolean? RequireCustomMerchantNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [require custom terminal number].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require custom terminal number]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [JsonProperty("require_custom_terminal_number")]
        public Boolean? RequireCustomTerminalNumber { get; set; }

        #endregion
    }
}