using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Merchant;

[ExcludeFromCodeCoverage]
public class MerchantStatementLine
{
    #region Properties

    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    /// <value>
    /// The amount.
    /// </value>
    public Decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the date time.
    /// </summary>
    /// <value>
    /// The date time.
    /// </value>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public String Description { get; set; }

    /// <summary>
    /// Gets or sets the type of the line.
    /// </summary>
    /// <value>
    /// The type of the line.
    /// </value>
    public Int32 LineType { get; set; }

    #endregion
}