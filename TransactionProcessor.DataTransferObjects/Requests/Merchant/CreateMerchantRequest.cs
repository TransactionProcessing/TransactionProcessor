using System;
using System.ComponentModel.DataAnnotations;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateMerchantRequest
    {
        #region Properties

        public Address Address { get; set; }

        public Contact Contact { get; set; }

        public String Name { get; set; }
        
        public SettlementSchedule SettlementSchedule { get; set; }

        public Guid? MerchantId { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        #endregion
    }
}