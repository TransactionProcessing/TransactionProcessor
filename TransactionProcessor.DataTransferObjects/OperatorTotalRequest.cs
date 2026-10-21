namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    /// <summary>Data transfer object for OperatorTotalRequest.</summary>
    /// <example>{"contractId":"00000000-0000-0000-0000-000000000239","operatorId":"00000000-0000-0000-0000-000000000253","transactionCount":1,"transactionValue":42.5}</example>
    
    [ExcludeFromCodeCoverage]
    public class OperatorTotalRequest
    {
        #region Properties

        /// <summary>
        /// Contract associated with the product or transaction.
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Operator associated with the operation.
        /// </summary>
        public Guid OperatorId { get; set; }

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
