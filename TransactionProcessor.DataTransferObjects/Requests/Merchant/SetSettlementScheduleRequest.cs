using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    [ExcludeFromCodeCoverage]
    public class SetSettlementScheduleRequest
    {
        [JsonProperty("settlment_schedule")]
        public SettlementSchedule SettlementSchedule { get; set; }

    }
}
