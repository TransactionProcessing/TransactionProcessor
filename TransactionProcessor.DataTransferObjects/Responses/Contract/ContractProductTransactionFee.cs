using System;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    public class ContractProductTransactionFee
    {
        #region Properties

        /// <summary>
        /// Gets or sets the type of the calculation.
        /// </summary>
        /// <value>
        /// The type of the calculation.
        /// </value>
        [JsonProperty("calculation_type")]
        public CalculationType CalculationType { get; set; }

        /// <summary>
        /// Gets or sets the type of the fee.
        /// </summary>
        /// <value>
        /// The type of the fee.
        /// </value>
        [JsonProperty("fee_type")]
        public FeeType FeeType { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [JsonProperty("description")]
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the transaction fee identifier.
        /// </summary>
        /// <value>
        /// The transaction fee identifier.
        /// </value>
        [JsonProperty("transaction_fee_id")]
        public Guid TransactionFeeId { get; set; }

        [JsonProperty("transaction_fee_reporting_id")]
        public Int32 TransactionFeeReportingId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [JsonProperty("value")]
        public Decimal Value { get; set; }

        #endregion
    }
}