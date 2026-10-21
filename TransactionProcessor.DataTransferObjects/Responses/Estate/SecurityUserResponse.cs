using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>Data transfer object for SecurityUserResponse.</summary>
    /// <example>{"emailAddress":"customer@example.com","securityUserId":"00000000-0000-0000-0000-000000000170"}</example>
    [ExcludeFromCodeCoverage]
    public class SecurityUserResponse
    {
        #region Properties
        /// <summary>The email address.</summary>

        public String EmailAddress { get; set; }
        /// <summary>The security user id.</summary>

        public Guid SecurityUserId { get; set; }

        #endregion
    }
}
