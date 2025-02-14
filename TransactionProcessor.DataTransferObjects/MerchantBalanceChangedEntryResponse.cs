using System;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class MerchantBalanceChangedEntryResponse
    {
        [JsonProperty("original_event_id")]
        public Guid OriginalEventId { get; set; }

        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        [JsonProperty("change_amount")]
        public Decimal ChangeAmount { get; set; }

        [JsonProperty("date_time")]
        public DateTime DateTime { get; set; }

        [JsonProperty("reference")]
        public String Reference { get; set; }

        [JsonProperty("debit_or_credit")]
        public String DebitOrCredit { get; set; }

        [JsonProperty("balance")]
        public Decimal Balance { get; set; }
    }
}
