using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Estate
{
    /// <summary>
    /// Login credentials and personal name fields for a new estate user.
    /// </summary>
    /// <example>{"emailAddress":"customer@example.com","password":"example-password","givenName":"Example Name","middleName":"Example Name","familyName":"Example Name"}</example>
    [ExcludeFromCodeCoverage]
    public class CreateEstateUserRequest
    {
        /// <summary>
        /// Email address for the user or contact.
        /// </summary>
        public String EmailAddress { get; set; }

        /// <summary>
        /// Password for the user account.
        /// </summary>
        public String Password { get; set; }
        
        /// <summary>
        /// Given name of the user.
        /// </summary>
        public String GivenName { get; set; }

        /// <summary>
        /// Middle name of the user, if applicable.
        /// </summary>
        public String MiddleName { get; set; }

        /// <summary>
        /// Family name of the user.
        /// </summary>
        public String FamilyName { get; set; }
    }
}
