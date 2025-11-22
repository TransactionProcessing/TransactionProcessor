using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    [ExcludeFromCodeCoverage]
    public class AddressResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the address identifier.
        /// </summary>
        /// <value>
        /// The address identifier.
        /// </value>
        [JsonProperty("address_id")]
        public Guid AddressId { get; set; }

        /// <summary>
        /// Gets the address line1.
        /// </summary>
        /// <value>
        /// The address line1.
        /// </value>
        [JsonProperty("address_line1")]
        public String AddressLine1 { get; set; }

        /// <summary>
        /// Gets the address line2.
        /// </summary>
        /// <value>
        /// The address line2.
        /// </value>
        [JsonProperty("address_line2")]
        public String AddressLine2 { get; set; }

        /// <summary>
        /// Gets the address line3.
        /// </summary>
        /// <value>
        /// The address line3.
        /// </value>
        [JsonProperty("address_line3")]
        public String AddressLine3 { get; set; }

        /// <summary>
        /// Gets the address line4.
        /// </summary>
        /// <value>
        /// The address line4.
        /// </value>
        [JsonProperty("address_line4")]
        public String AddressLine4 { get; set; }

        /// <summary>
        /// Gets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        [JsonProperty("country")]
        public String Country { get; set; }

        /// <summary>
        /// Gets the postal code.
        /// </summary>
        /// <value>
        /// The postal code.
        /// </value>
        [JsonProperty("postal_code")]
        public String PostalCode { get; set; }

        /// <summary>
        /// Gets the region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        [JsonProperty("region")]
        public String Region { get; set; }

        /// <summary>
        /// Gets the town.
        /// </summary>
        /// <value>
        /// The town.
        /// </value>
        [JsonProperty("town")]
        public String Town { get; set; }

        #endregion
    }
}