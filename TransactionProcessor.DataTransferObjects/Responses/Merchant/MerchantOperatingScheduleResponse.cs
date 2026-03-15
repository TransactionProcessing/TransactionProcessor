using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    public class MerchantOperatingScheduleResponse
    {
        [JsonProperty("year")]
        public Int32 Year { get; set; }

        [JsonProperty("default_is_open")]
        public Boolean DefaultIsOpen { get; set; }

        [JsonProperty("periods")]
        public List<MerchantOperatingSchedulePeriodResponse> Periods { get; set; } = new();
    }
}
