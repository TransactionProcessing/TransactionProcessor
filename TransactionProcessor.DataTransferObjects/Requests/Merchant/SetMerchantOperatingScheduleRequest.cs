using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    public class SetMerchantOperatingScheduleRequest
    {
        [JsonProperty("default_is_open")]
        public Boolean DefaultIsOpen { get; set; }

        [JsonProperty("periods")]
        public List<MerchantOperatingSchedulePeriodRequest> Periods { get; set; } = new();
    }
}
