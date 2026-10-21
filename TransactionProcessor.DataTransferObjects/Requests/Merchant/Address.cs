using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Postal address fields used when adding or updating a merchant address.
    /// </summary>
    /// <example>{"addressLine1":"123 Example Street","addressLine2":"123 Example Street","addressLine3":"123 Example Street","addressLine4":"123 Example Street","country":"US","postalCode":"10001","region":"Example City","town":"Example City"}</example>
    public class Address
    {
        #region Properties

        /// <summary>
        /// First line of the postal address.
        /// </summary>
        public String AddressLine1 { get; set; }

        /// <summary>
        /// Second line of the postal address.
        /// </summary>
        public String AddressLine2 { get; set; }

        /// <summary>
        /// Third line of the postal address.
        /// </summary>
        public String AddressLine3 { get; set; }

        /// <summary>
        /// Fourth line of the postal address.
        /// </summary>
        public String AddressLine4 { get; set; }

        /// <summary>
        /// Country for the postal address.
        /// </summary>
        public String Country { get; set; }

        /// <summary>
        /// Postal or ZIP code for the address.
        /// </summary>
        public String PostalCode { get; set; }

        /// <summary>
        /// State, county, or region for the address.
        /// </summary>
        public String Region { get; set; }
        /// <summary>
        /// Town or city for the address.
        /// </summary>
        public String Town { get; set; }

        #endregion
    }
}
