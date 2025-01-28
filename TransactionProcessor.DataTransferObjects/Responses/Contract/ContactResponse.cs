using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    [ExcludeFromCodeCoverage]
    public class ContactResponse
    {
        #region Properties

        /// <summary>
        /// Gets the contact email address.
        /// </summary>
        /// <value>
        /// The contact email address.
        /// </value>
        [JsonProperty("contact_email_address")]
        public String ContactEmailAddress { get; set; }

        /// <summary>
        /// Gets the contact identifier.
        /// </summary>
        /// <value>
        /// The contact identifier.
        /// </value>
        [JsonProperty("contact_id")]
        public Guid ContactId { get; set; }

        /// <summary>
        /// Gets the name of the contact.
        /// </summary>
        /// <value>
        /// The name of the contact.
        /// </value>
        [JsonProperty("contact_name")]
        public String ContactName { get; set; }

        /// <summary>
        /// Gets the contact phone number.
        /// </summary>
        /// <value>
        /// The contact phone number.
        /// </value>
        [JsonProperty("contact_phone_number")]
        public String ContactPhoneNumber { get; set; }

        #endregion
    }
}