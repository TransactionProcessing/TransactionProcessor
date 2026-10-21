using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>Data transfer object for ContactResponse.</summary>
    /// <example>{"contactEmailAddress":"customer@example.com","contactId":"00000000-0000-0000-0000-000000000125","contactName":"Example Name","contactPhoneNumber":"+15550100"}</example>
    [ExcludeFromCodeCoverage]
    public class ContactResponse
    {
        #region Properties
        /// <summary>The contact email address.</summary>
        public String ContactEmailAddress { get; set; }
        /// <summary>The contact id.</summary>

        public Guid ContactId { get; set; }
        /// <summary>The contact name.</summary>

        public String ContactName { get; set; }
        /// <summary>The contact phone number.</summary>

        public String ContactPhoneNumber { get; set; }

        #endregion
    }
}
