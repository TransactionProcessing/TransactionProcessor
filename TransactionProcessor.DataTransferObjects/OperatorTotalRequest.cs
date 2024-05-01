namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OperatorTotalRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        /// <value>
        /// The contract identifier.
        /// </value>
        [JsonProperty("contract_id")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        [JsonProperty("operator_id")]
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Gets or sets the transaction count.
        /// </summary>
        /// <value>
        /// The transaction count.
        /// </value>
        [JsonProperty("transaction_count")]
        public Int32 TransactionCount { get; set; }

        /// <summary>
        /// Gets or sets the transaction value.
        /// </summary>
        /// <value>
        /// The transaction value.
        /// </value>
        [JsonProperty("transaction_cvalue")]
        public Decimal TransactionValue { get; set; }

        #endregion
    }
}