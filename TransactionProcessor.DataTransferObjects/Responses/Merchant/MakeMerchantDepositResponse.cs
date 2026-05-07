using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    [ExcludeFromCodeCoverage]
    public class MakeMerchantDepositResponse
    {
        #region Properties

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public Guid DepositId { get; set; }

        #endregion
    }
}
