using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateEstateUserResponse
    {
        /// <summary>
        /// Gets or sets the name of the estate.
        /// </summary>
        /// <value>
        /// The name of the estate.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }
    }
}