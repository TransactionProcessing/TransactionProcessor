using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Operator
{
    [ExcludeFromCodeCoverage]
    public class CreateOperatorResponse
    {
        #region Properties

        public Guid EstateId { get; set; }

        public Guid OperatorId { get; set; }

        #endregion
    }
}