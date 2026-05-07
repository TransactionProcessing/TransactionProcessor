using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    [ExcludeFromCodeCoverage]
    public class ContactResponse
    {
        #region Properties
        public String ContactEmailAddress { get; set; }

        public Guid ContactId { get; set; }

        public String ContactName { get; set; }

        public String ContactPhoneNumber { get; set; }

        #endregion
    }
}