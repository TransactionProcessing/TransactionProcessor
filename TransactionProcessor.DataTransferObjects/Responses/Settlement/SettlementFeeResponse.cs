using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Settlement
{
    [ExcludeFromCodeCoverage]
    public class SettlementFeeResponse
    {
        #region Properties

        [JsonProperty("calculated_value")]
        public Decimal CalculatedValue { get; set; }

        [JsonProperty("fee_description")]
        public String FeeDescription { get; set; }

        [JsonProperty("is_settled")]
        public Boolean IsSettled { get; set; }

        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        [JsonProperty("merchant_name")]
        public String MerchantName { get; set; }

        [JsonProperty("settlement_date")]
        public DateTime SettlementDate { get; set; }

        [JsonProperty("settlement_id")]
        public Guid SettlementId { get; set; }

        [JsonProperty("transaction_id")]
        public Guid TransactionId { get; set; }

        [JsonProperty("operator_identifier")]
        public String OperatorIdentifier { get; set; }

        #endregion
    }
}