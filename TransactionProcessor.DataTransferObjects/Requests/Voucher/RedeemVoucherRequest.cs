namespace TransactionProcessor.DataTransferObjects.Requests.Voucher
{
    using System;

    /// <summary>
    /// Host timestamp, voucher code, and estate to redeem, with an optional redemption timestamp.
    /// </summary>
    /// <example>{"hostDateTime":"2026-01-15T10:30:00Z","voucherCode":"VOUCHER-2026-001","estateId":"00000000-0000-0000-0000-000000000023","redeemedDateTime":"2026-01-15T10:30:00Z"}</example>
    public class RedeemVoucherRequest : RequestWithHostDateTime {
        /// <summary>
        /// Code printed on or assigned to the voucher.
        /// </summary>
        public String VoucherCode { get; set; }

        /// <summary>
        /// Estate that owns or scopes this operation.
        /// </summary>
        public Guid EstateId { get; set; }

        /// <summary>
        /// Optional date and time when the voucher was redeemed; omitted values use the current local time.
        /// </summary>
        public DateTimeOffset? RedeemedDateTime { get; set; }
    }
}
