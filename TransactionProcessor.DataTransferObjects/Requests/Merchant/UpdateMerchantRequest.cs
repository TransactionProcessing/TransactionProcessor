using System;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Opening and closing times by day of week for a merchant.
    /// </summary>
    /// <example>{"name":"Example Name","settlementSchedule":0}</example>
    public class UpdateMerchantRequest
    {
        /// <summary>
        /// Display name for the estate, merchant, or operator.
        /// </summary>
        public String Name { get; set; }
        
        /// <summary>
        /// Settlement schedule assigned to the merchant.
        /// </summary>
        public SettlementSchedule SettlementSchedule { get; set; }
    }
}
