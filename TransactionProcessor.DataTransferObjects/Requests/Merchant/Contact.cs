using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Contact name, email address, and phone number for a merchant contact.
    /// </summary>
    /// <example>{"contactName":"Example Name","emailAddress":"customer@example.com","phoneNumber":"+15550100"}</example>
    public class Contact
    {
        #region Properties

        /// <summary>
        /// Name of the contact person.
        /// </summary>
        public String ContactName { get; set; }

        /// <summary>
        /// Email address for the user or contact.
        /// </summary>
        public String EmailAddress { get; set; }

        /// <summary>
        /// Telephone number for the contact person.
        /// </summary>
        public String PhoneNumber { get; set; }

        #endregion
    }
}
