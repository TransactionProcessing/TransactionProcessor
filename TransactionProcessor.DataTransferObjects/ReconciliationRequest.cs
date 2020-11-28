namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

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
        public String DeviceIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the operator totals.
        /// </summary>
        /// <value>
        /// The operator totals.
        /// </value>
        public List<OperatorTotalRequest> OperatorTotals { get; set; }

        /// <summary>
        /// Gets or sets the transaction count.
        /// </summary>
        /// <value>
        /// The transaction count.
        /// </value>
        public Int32 TransactionCount { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the transaction value.
        /// </summary>
        /// <value>
        /// The transaction value.
        /// </value>
        public Decimal TransactionValue { get; set; }

        #endregion
    }
}