using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    public class Contact
    {
        #region Properties

        public String ContactName { get; set; }

        public String EmailAddress { get; set; }

        public String PhoneNumber { get; set; }

        #endregion
    }
}