using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateEstateUserResponse
    {
        public Guid EstateId { get; set; }

        public Guid UserId { get; set; }
    }
}