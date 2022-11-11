namespace TransactionProcessor.Models;

using System;

public class RedeemVoucherResponse
{
    #region Properties

    /// <summary>
    /// Gets or sets the expiry date.
    /// </summary>
    /// <value>
    /// The expiry date.
    /// </value>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the remaining balance.
    /// </summary>
    /// <value>
    /// The remaining balance.
    /// </value>
    public Decimal RemainingBalance { get; set; }

    /// <summary>
    /// Gets or sets the voucher code.
    /// </summary>
    /// <value>
    /// The voucher code.
    /// </value>
    public String VoucherCode { get; set; }

    #endregion
}