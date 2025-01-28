using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    [ExcludeFromCodeCoverage]
    public class CreateMerchantUserRequest
    {
        [Required]
        [JsonProperty("email_address")]
        public String EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [Required]
        [JsonProperty("password")]
        public String Password { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the given.
        /// </summary>
        /// <value>
        /// The name of the given.
        /// </value>
        [Required]
        [JsonProperty("given_name")]
        public String GivenName { get; set; }

        /// <summary>
        /// Gets or sets the name of the middle.
        /// </summary>
        /// <value>
        /// The name of the middle.
        /// </value>
        [JsonProperty("middle_name")]
        public String MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the name of the family.
        /// </summary>
        /// <value>
        /// The name of the family.
        /// </value>
        [Required]
        [JsonProperty("family_name")]
        public String FamilyName { get; set; }
    }
}
