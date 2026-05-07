using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MakeMerchantDepositRequest
    {
        #region Properties

        public Decimal Amount { get; set; }

        public DateTime DepositDateTime { get; set; }

        public String Reference { get; set; }

        #endregion
    }
}