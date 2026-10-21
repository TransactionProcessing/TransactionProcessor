namespace TransactionProcessor.DataTransferObjects
{
    using System;
    /// <summary>
    /// Voucher code, expiry date, and remaining balance after redemption.
    /// </summary>
    /// <example>{"expiryDate":"2026-01-15T10:30:00Z","voucherCode":"VOUCHER-2026-001","remainingBalance":42.5}</example>
    public class RedeemVoucherResponse
    {
        /// <summary>The expiry date.</summary>
        public DateTime ExpiryDate { get; set; }
        /// <summary>The voucher code.</summary>

        public String VoucherCode { get; set; }
        /// <summary>The remaining balance.</summary>

        public Decimal RemainingBalance { get; set; }
    }
}
