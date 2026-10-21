using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Data transfer object for MakeMerchantDepositResponse.</summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","depositId":"00000000-0000-0000-0000-000000000137"}</example>
    [ExcludeFromCodeCoverage]
    public class MakeMerchantDepositResponse
    {
        #region Properties
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The deposit id.</summary>

        public Guid DepositId { get; set; }

        #endregion
    }
}
