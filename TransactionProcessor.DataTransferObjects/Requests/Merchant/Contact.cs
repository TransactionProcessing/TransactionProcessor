using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    public class Contact
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the contact.
        /// </summary>
        /// <value>
        /// The name of the contact.
        /// </value>
        [JsonProperty("contact_name")]
        [Required]
        public String ContactName { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        [JsonProperty("email_address")]
        [Required]
        public String EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [JsonProperty("phone_number")]
        public String PhoneNumber { get; set; }

        #endregion
    }
}