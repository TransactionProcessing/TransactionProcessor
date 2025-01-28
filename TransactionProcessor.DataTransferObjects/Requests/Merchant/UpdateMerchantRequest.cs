using System;
using Newtonsoft.Json;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    public class UpdateMerchantRequest
    {
        #region Properties

        [JsonProperty("name")]
        public String Name { get; set; }
        
        [JsonProperty("settlement_schedule")]
        public SettlementSchedule SettlementSchedule { get; set; }

        #endregion
    }
}