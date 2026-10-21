using System;
using System.ComponentModel.DataAnnotations;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Merchant identity, contact, address, and settlement schedule used at creation.
    /// </summary>
    /// <example>{"address":{"addressLine1":"123 Example Street","addressLine2":"123 Example Street","addressLine3":"123 Example Street","addressLine4":"123 Example Street","country":"US","postalCode":"10001","region":"Example City","town":"Example City"},"contact":{"contactName":"Example Name","emailAddress":"customer@example.com","phoneNumber":"+15550100"},"name":"Example Name","settlementSchedule":0,"merchantId":"00000000-0000-0000-0000-000000000227","createdDateTime":"2026-01-15T10:30:00Z"}</example>
    public class CreateMerchantRequest
    {
        #region Properties

        /// <summary>
        /// Postal address for the merchant.
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Primary contact for the merchant.
        /// </summary>
        public Contact Contact { get; set; }

        /// <summary>
        /// Display name for the estate, merchant, or operator.
        /// </summary>
        public String Name { get; set; }
        
        /// <summary>
        /// Settlement schedule assigned to the merchant.
        /// </summary>
        public SettlementSchedule SettlementSchedule { get; set; }

        /// <summary>
        /// Optional caller-supplied merchant identifier; when omitted the service creates one.
        /// </summary>
        public Guid? MerchantId { get; set; }

        /// <summary>
        /// Optional merchant creation timestamp.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        #endregion
    }
}
