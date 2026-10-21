namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    
    /// <summary>
    /// Terminal transaction totals and optional per-operator/per-contract breakdown.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","transactionDateTime":"2026-01-15T10:30:00Z","transactionType":"example","deviceIdentifier":"EXAMPLE-001","operatorTotals":[{"contractId":"00000000-0000-0000-0000-000000000239","operatorId":"00000000-0000-0000-0000-000000000253","transactionCount":1,"transactionValue":42.5}],"transactionCount":1,"transactionValue":42.5}</example>
    [ExcludeFromCodeCoverage]
    public class ReconciliationRequest : TransactionRequest
    {
        #region Properties

        /// <summary>
        /// Identifier assigned to the terminal device submitting the reconciliation.
        /// </summary>
        public String DeviceIdentifier { get; set; }

        /// <summary>
        /// Optional transaction totals grouped by operator and contract.
        /// </summary>
        public List<OperatorTotalRequest> OperatorTotals { get; set; }

        /// <summary>
        /// Number of transactions included in the reconciliation.
        /// </summary>
        public Int32 TransactionCount { get; set; }

        /// <summary>
        /// Total value of transactions included in the reconciliation.
        /// </summary>
        public Decimal TransactionValue { get; set; }

        #endregion
    }
}
