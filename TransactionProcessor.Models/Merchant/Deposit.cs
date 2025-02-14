using System;

namespace TransactionProcessor.Models.Merchant
{
    public class Deposit
    {
        #region Properties

        public Decimal Amount { get; set; }

        public DateTime DepositDateTime { get; set; }

        public Guid DepositId { get; set; }

        public String Reference { get; set; }

        public MerchantDepositSource Source { get; set; }

        #endregion
    }
}