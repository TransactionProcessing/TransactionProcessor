namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TransactionProcessor.DataTransferObjects.DataTransferObject" />
    [ExcludeFromCodeCoverage]
    public class ReconciliationRequest : DataTransferObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        [JsonProperty("device_identifier")]
        public String DeviceIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the operator totals.
        /// </summary>
        /// <value>
        /// The operator totals.
        /// </value>
        [JsonProperty("operator_totals")]
        public List<OperatorTotalRequest> OperatorTotals { get; set; }

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
        [JsonProperty("transaction_value")]
        public Decimal TransactionValue { get; set; }

        #endregion
    }
}