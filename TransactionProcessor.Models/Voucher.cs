namespace TransactionProcessor.Models;

using System;

public class Voucher
{
    #region Properties

    /// <summary>
    /// Gets the barcode.
    /// </summary>
    /// <value>
    /// The barcode.
    /// </value>
    public String Barcode { get; set; }

    /// <summary>
    /// Gets the estate identifier.
    /// </summary>
    /// <value>
    /// The estate identifier.
    /// </value>
    public Guid EstateId { get; set; }

    /// <summary>
    /// Gets the expiry date.
    /// </summary>
    /// <value>
    /// The expiry date.
    /// </value>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance is generated.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is generated; otherwise, <c>false</c>.
    /// </value>
    public Boolean IsGenerated { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance is issued.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is issued; otherwise, <c>false</c>.
    /// </value>
    public Boolean IsIssued { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is redeemed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is redeemed; otherwise, <c>false</c>.
    /// </value>
    public Boolean IsRedeemed { get; set; }

    /// <summary>
    /// Gets the issued date time.
    /// </summary>
    /// <value>
    /// The issued date time.
    /// </value>
    public DateTime IssuedDateTime { get; set; }
    /// <summary>
    /// Gets or sets the generated date time.
    /// </summary>
    /// <value>
    /// The generated date time.
    /// </value>
    public DateTime GeneratedDateTime { get; set; }
    /// <summary>
    /// Gets or sets the redeemed date time.
    /// </summary>
    /// <value>
    /// The redeemed date time.
    /// </value>
    public DateTime RedeemedDateTime { get; set; }

    /// <summary>
    /// Gets the message.
    /// </summary>
    /// <value>
    /// The message.
    /// </value>
    public String Message { get; set; }

    /// <summary>
    /// The recipient email
    /// </summary>
    /// <value>
    /// The recipient email.
    /// </value>
    public String RecipientEmail { get; set; }

    /// <summary>
    /// The recipient mobile
    /// </summary>
    /// <value>
    /// The recipient mobile.
    /// </value>
    public String RecipientMobile { get; set; }

    /// <summary>
    /// Gets the transaction identifier.
    /// </summary>
    /// <value>
    /// The transaction identifier.
    /// </value>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    public Decimal Value { get; set; }

    /// <summary>
    /// Gets or sets the balance.
    /// </summary>
    /// <value>
    /// The balance.
    /// </value>
    public Decimal Balance { get; set; }

    /// <summary>
    /// Gets the voucher code.
    /// </summary>
    /// <value>
    /// The voucher code.
    /// </value>
    public String VoucherCode { get; set; }

    /// <summary>
    /// Gets or sets the voucher identifier.
    /// </summary>
    /// <value>
    /// The voucher identifier.
    /// </value>
    public Guid VoucherId { get; set; }

    #endregion
}