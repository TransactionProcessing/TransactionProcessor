using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Contract
{
    /// <summary>
    /// Information needed to create a contract and associate it with an estate operator.
    /// </summary>
    /// <example>{"operatorId":"00000000-0000-0000-0000-000000000253","description":"Example description"}</example>
    [ExcludeFromCodeCoverage]
    public class CreateContractRequest
    {
        /// <summary>
        /// Operator associated with the operation.
        /// </summary>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Human-readable description of the contract or fee.
        /// </summary>
        public String Description { get; set; }
    }
}
