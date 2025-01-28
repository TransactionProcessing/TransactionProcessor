using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    [ExcludeFromCodeCoverage]
    public class SecurityUserResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        [JsonProperty("email_address")]
        public String EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the security user identifier.
        /// </summary>
        /// <value>
        /// The security user identifier.
        /// </value>
        [JsonProperty("security_user_id")]
        public Guid SecurityUserId { get; set; }

        #endregion
    }
}