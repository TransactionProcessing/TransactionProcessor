using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    [ExcludeFromCodeCoverage]
    public class CreateEstateResponse
    {
        public Guid EstateId { get; set; }
    }
}