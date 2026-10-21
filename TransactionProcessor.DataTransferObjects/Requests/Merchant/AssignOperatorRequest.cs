using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Operator identifier and merchant or terminal identifiers to assign to a merchant.
    /// </summary>
    /// <example>{"merchantNumber":"EXAMPLE-001","operatorId":"00000000-0000-0000-0000-000000000253","terminalNumber":"EXAMPLE-001"}</example>
    [ExcludeFromCodeCoverage]
    public class AssignOperatorRequest
    {
        #region Properties

        /// <summary>
        /// Merchant number used when this operator is assigned.
        /// </summary>
        public String MerchantNumber { get; set; }

        /// <summary>
        /// Operator associated with the operation.
        /// </summary>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Terminal number used when this operator is assigned.
        /// </summary>
        public String TerminalNumber { get; set; }

        #endregion
    }
}
