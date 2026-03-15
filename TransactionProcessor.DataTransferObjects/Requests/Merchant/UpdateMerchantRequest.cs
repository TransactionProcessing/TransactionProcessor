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

    public class MerchantOpeningRequest {
        public OpeningHours Sunday { get; set; }
        public OpeningHours Monday { get; set; }
        public OpeningHours Tuesday { get; set; }
        public OpeningHours Wednesday { get; set; }
        public OpeningHours Thursday { get; set; }
        public OpeningHours Friday { get; set; }
        public OpeningHours Saturday { get; set; }
    }


    public class OpeningHours
    {
        public string Opening { get; set; }
        public string Closing { get; set; }
    }

    public class OpeningHoursResponse
    {
        public string Opening { get; set; }
        public string Closing { get; set; }
    }
}