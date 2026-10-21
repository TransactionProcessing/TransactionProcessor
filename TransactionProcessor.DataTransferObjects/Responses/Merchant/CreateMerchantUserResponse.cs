using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Data transfer object for CreateMerchantUserResponse.</summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","userId":"00000000-0000-0000-0000-000000000079"}</example>
    [ExcludeFromCodeCoverage]
    public class CreateMerchantUserResponse
    {
        #region Properties
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The user id.</summary>

        public Guid UserId { get; set; }

        #endregion
    }
}
