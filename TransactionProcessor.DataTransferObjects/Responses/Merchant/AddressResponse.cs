using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Data transfer object for AddressResponse.</summary>
    /// <example>{"addressId":"00000000-0000-0000-0000-000000000119","addressLine1":"123 Example Street","addressLine2":"123 Example Street","addressLine3":"123 Example Street","addressLine4":"123 Example Street","country":"US","postalCode":"10001","region":"Example City","town":"Example City"}</example>
    [ExcludeFromCodeCoverage]
    public class AddressResponse
    {
        #region Properties
        /// <summary>The address id.</summary>

        public Guid AddressId { get; set; }
        /// <summary>The address line1.</summary>

        public String AddressLine1 { get; set; }
        /// <summary>The address line2.</summary>

        public String AddressLine2 { get; set; }
        /// <summary>The address line3.</summary>

        public String AddressLine3 { get; set; }
        /// <summary>The address line4.</summary>

        public String AddressLine4 { get; set; }
        /// <summary>The country.</summary>

        public String Country { get; set; }
        /// <summary>The postal code.</summary>

        public String PostalCode { get; set; }
        /// <summary>The region.</summary>

        public String Region { get; set; }
        /// <summary>The town.</summary>

        public String Town { get; set; }

        #endregion
    }
}
