using System;

namespace TransactionProcessor.DataTransferObjects
{
    /// <summary>
    /// Voucher identity, value, balance, lifecycle status, dates, and associated transaction.
    /// </summary>
    /// <example>{"voucherId":"00000000-0000-0000-0000-000000000141","expiryDate":"2026-01-15T10:30:00Z","value":42.5,"balance":42.5,"voucherCode":"VOUCHER-2026-001","isGenerated":false,"isIssued":false,"isRedeemed":false,"issuedDateTime":"2026-01-15T10:30:00Z","generatedDateTime":"2026-01-15T10:30:00Z","redeemedDateTime":"2026-01-15T10:30:00Z","transactionId":"00000000-0000-0000-0000-000000000057"}</example>
    public class GetVoucherResponse
    {
        /// <summary>The voucher id.</summary>
        public Guid VoucherId { get; set; }
        /// <summary>The expiry date.</summary>

        public DateTime ExpiryDate { get; set; }
        /// <summary>The value.</summary>

        public Decimal Value { get; set; }
        /// <summary>The balance.</summary>

        public Decimal Balance { get; set; }
        /// <summary>The voucher code.</summary>

        public String VoucherCode { get; set; }
        /// <summary>The is generated.</summary>

        public Boolean IsGenerated { get; set; }
        /// <summary>The is issued.</summary>

        public Boolean IsIssued { get; set; }
        /// <summary>The is redeemed.</summary>
        public Boolean IsRedeemed { get; set; }
        /// <summary>The issued date time.</summary>

        public DateTime IssuedDateTime { get; set; }
        /// <summary>The generated date time.</summary>

        public DateTime GeneratedDateTime { get; set; }
        /// <summary>The redeemed date time.</summary>
        
        public DateTime RedeemedDateTime { get; set; }
        /// <summary>The transaction id.</summary>
        
        public Guid TransactionId { get; set; }
    }
}
