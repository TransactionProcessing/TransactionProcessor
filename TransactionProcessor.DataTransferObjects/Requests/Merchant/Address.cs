using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    public class Address
    {
        #region Properties

        public String AddressLine1 { get; set; }

        public String AddressLine2 { get; set; }

        public String AddressLine3 { get; set; }

        public String AddressLine4 { get; set; }

        public String Country { get; set; }

        public String PostalCode { get; set; }

        public String Region { get; set; }
        public String Town { get; set; }

        #endregion
    }
}