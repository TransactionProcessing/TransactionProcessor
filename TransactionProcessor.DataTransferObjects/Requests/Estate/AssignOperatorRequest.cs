using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Estate
{
    /// <summary>
    /// Operator identifier to associate with an estate.
    /// </summary>
    /// <example>{"operatorId":"00000000-0000-0000-0000-000000000253"}</example>
    [ExcludeFromCodeCoverage]
    public class AssignOperatorRequest
    {
        #region Properties

        /// <summary>
        /// Operator associated with the operation.
        /// </summary>
        public Guid OperatorId { get; set; }

        #endregion
    }
}
