namespace TransactionProcessor.DataTransferObjects
{
    using System;
    /// <summary>Data transfer object for IssueVoucherResponse.</summary>
    /// <example>{"voucherId":"00000000-0000-0000-0000-000000000141","expiryDate":"2026-01-15T10:30:00Z","voucherCode":"VOUCHER-2026-001","message":"Example message"}</example>

    public class IssueVoucherResponse
    {
        /// <summary>The voucher id.</summary>
        public Guid VoucherId { get; set; }
        /// <summary>The expiry date.</summary>

        public DateTime ExpiryDate { get; set; }
        /// <summary>The voucher code.</summary>

        public String VoucherCode { get; set; }
        /// <summary>The message.</summary>

        public String Message { get; set; }
    }
}
