using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Estate
{
    public class CreateEstateRequest
    {
        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [Required]
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the name of the estate.
        /// </summary>
        /// <value>
        /// The name of the estate.
        /// </value>
        [Required]
        [JsonProperty("estate_name")]
        public String EstateName { get; set; }
    }
}