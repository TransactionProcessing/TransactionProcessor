namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    [ExcludeFromCodeCoverage]
    public class SaleTransactionRequest : DataTransferObject
    {
        #region Properties

        [JsonProperty("additional_transaction_metadata")]
        public Dictionary<String, String> AdditionalTransactionMetadata { get; set; }

        [JsonProperty("contact_id")]
        public Guid ContractId { get; set; }

        [JsonProperty("customer_email_address")]
        public String CustomerEmailAddress { get; set; }

        [JsonProperty("device_identifier")]
        public String DeviceIdentifier { get; set; }

        [JsonProperty("operator_identifier")]
        public String OperatorIdentifier { get; set; }

        [JsonProperty("product_id")]
        public Guid ProductId { get; set; }

        [JsonProperty("transaction_date_time")]
        public DateTime TransactionDateTime { get; set; }

        [JsonProperty("transaction_number")]
        public String TransactionNumber { get; set; }

        [JsonProperty("transaction_source")]
        public Int32? TransactionSource { get; set; }

        [JsonProperty("transaction_type")]
        public String TransactionType { get; set; }

        #endregion
    }
}