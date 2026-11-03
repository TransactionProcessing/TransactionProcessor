using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Transaction;

/// <summary>
/// Shared host, estate, merchant, transaction timestamp, and transaction type fields.
/// </summary>
/// <example>{"hostDateTime":"2026-01-15T10:30:00Z","estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","transactionDateTime":"2026-01-15T10:30:00Z","transactionType":"example"}</example>
[ExcludeFromCodeCoverage]
public class TransactionRequest : RequestWithHostDateTime {
    /// <summary>
    /// Estate that owns or scopes this operation.
    /// </summary>
    public Guid EstateId { get; set; }

    /// <summary>
    /// Merchant associated with the transaction.
    /// </summary>
    public Guid MerchantId { get; set; }

    /// <summary>
    /// Date and time when the transaction occurred at the terminal.
    /// </summary>
    public DateTimeOffset TransactionDateTime { get; set; }

    /// <summary>
    /// Transaction type reported by the terminal.
    /// </summary>
    public String TransactionType { get; set; }
}
