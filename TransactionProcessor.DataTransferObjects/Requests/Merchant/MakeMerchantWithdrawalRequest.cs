using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    [ExcludeFromCodeCoverage]
    public class MakeMerchantWithdrawalRequest
    {
        #region Properties

        public Decimal Amount { get; set; }

        public DateTime WithdrawalDateTime { get; set; }

        public String Reference { get; set; }

        #endregion
    }
}