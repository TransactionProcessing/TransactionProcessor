namespace TransactionProcessor.DataTransferObjects
{
    using System;
    public class RedeemVoucherResponse
    {
        public DateTime ExpiryDate { get; set; }

        public String VoucherCode { get; set; }

        public Decimal RemainingBalance { get; set; }
    }
}