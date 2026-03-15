using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.MerchantSchedule
{
    [ExcludeFromCodeCoverage]
    public class CreateMerchantScheduleRequest
    {
        [JsonProperty("year")]
        public Int32 Year { get; set; }

        [JsonProperty("months")]
        public List<MerchantScheduleMonthRequest> Months { get; set; } = [];
    }

    [ExcludeFromCodeCoverage]
    public class UpdateMerchantScheduleRequest
    {
        [JsonProperty("months")]
        public List<MerchantScheduleMonthRequest> Months { get; set; } = [];
    }

    [ExcludeFromCodeCoverage]
    public class MerchantScheduleMonthRequest
    {
        [JsonProperty("month")]
        public Int32 Month { get; set; }

        [JsonProperty("closed_days")]
        public List<Int32> ClosedDays { get; set; } = [];
    }
}
