using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Data transfer object for MakeMerchantWithdrawalResponse.</summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","withdrawalId":"00000000-0000-0000-0000-000000000201"}</example>
    [ExcludeFromCodeCoverage]
    public class MakeMerchantWithdrawalResponse
    {
        #region Properties
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The withdrawal id.</summary>

        public Guid WithdrawalId { get; set; }

        #endregion
    }
}
