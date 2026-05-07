using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    [ExcludeFromCodeCoverage]
    public class SecurityUserResponse
    {
        #region Properties

        public String EmailAddress { get; set; }

        public Guid SecurityUserId { get; set; }

        #endregion
    }
}