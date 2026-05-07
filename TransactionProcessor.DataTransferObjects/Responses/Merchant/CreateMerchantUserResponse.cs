using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    [ExcludeFromCodeCoverage]
    public class CreateMerchantUserResponse
    {
        #region Properties

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public Guid UserId { get; set; }

        #endregion
    }
}