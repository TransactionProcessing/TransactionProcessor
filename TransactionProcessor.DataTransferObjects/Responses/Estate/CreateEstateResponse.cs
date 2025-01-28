using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    [ExcludeFromCodeCoverage]
    public class CreateEstateResponse
    {
        /// <summary>
        /// Gets or sets the name of the estate.
        /// </summary>
        /// <value>
        /// The name of the estate.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }
    }
}