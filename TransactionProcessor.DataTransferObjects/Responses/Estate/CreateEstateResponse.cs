using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>Data transfer object for CreateEstateResponse.</summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023"}</example>
    [ExcludeFromCodeCoverage]
    public class CreateEstateResponse
    {
        /// <summary>The estate id.</summary>
        public Guid EstateId { get; set; }
    }
}
