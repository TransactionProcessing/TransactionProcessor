using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>
    /// Identifiers of the estate and newly created user.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","userId":"00000000-0000-0000-0000-000000000079"}</example>
    [ExcludeFromCodeCoverage]
    public class CreateEstateUserResponse
    {
        /// <summary>The estate id.</summary>
        public Guid EstateId { get; set; }
        /// <summary>The user id.</summary>

        public Guid UserId { get; set; }
    }
}
