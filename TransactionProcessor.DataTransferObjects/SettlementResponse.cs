using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class SettlementResponse
    {
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }
        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        [JsonProperty("settlement_date")]
        public DateTime SettlementDate { get; set; }

        [JsonProperty("number_of_fees_pending_settlement")]
        public Int32 NumberOfFeesPendingSettlement { get; set; }
        [JsonProperty("number_of_fees_settled")]
        public Int32 NumberOfFeesSettled { get; set; }
        [JsonProperty("settlement_completed")]
        public Boolean SettlementCompleted { get; set; }
    }
}