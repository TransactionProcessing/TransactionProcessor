using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Contract
{
    [ExcludeFromCodeCoverage]
    public class CreateContractRequest
    {
        public Guid OperatorId { get; set; }

        public String Description { get; set; }
    }
}
