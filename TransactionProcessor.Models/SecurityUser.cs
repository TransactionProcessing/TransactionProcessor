using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models
{
    [ExcludeFromCodeCoverage]
    public class SecurityUser
    {
        #region Properties

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        public String EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the security user identifier.
        /// </summary>
        /// <value>
        /// The security user identifier.
        /// </value>
        public Guid SecurityUserId { get; set; }

        #endregion
    }
}