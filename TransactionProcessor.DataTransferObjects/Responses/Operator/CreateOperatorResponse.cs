using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Operator
{
    /// <summary>Data transfer object for CreateOperatorResponse.</summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","operatorId":"00000000-0000-0000-0000-000000000253"}</example>
    [ExcludeFromCodeCoverage]
    public class CreateOperatorResponse
    {
        #region Properties
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The operator id.</summary>

        public Guid OperatorId { get; set; }

        #endregion
    }
}
