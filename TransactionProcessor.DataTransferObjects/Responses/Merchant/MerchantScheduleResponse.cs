using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant;

public class MerchantScheduleResponse
{
    [JsonProperty("year")]
    public Int32 Year { get; set; }

    [JsonProperty("months")]
    public List<MerchantScheduleMonthResponse> Months { get; set; } = [];
}

public class MerchantScheduleMonthResponse
{
    [JsonProperty("month")]
    public Int32 Month { get; set; }

    [JsonProperty("closed_days")]
    public List<Int32> ClosedDays { get; set; } = [];
}
