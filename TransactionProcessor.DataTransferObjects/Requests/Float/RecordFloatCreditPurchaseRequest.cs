using System;

namespace TransactionProcessor.DataTransferObjects.Requests.Float;

/// <summary>
/// Host timestamp, float credit amount, cost price, and purchase timestamp.
/// </summary>
/// <example>{"hostDateTime":"2026-01-15T10:30:00Z","floatId":"00000000-0000-0000-0000-000000000166","purchaseDateTime":"2026-01-15T10:30:00Z","creditAmount":42.5,"costPrice":42.5}</example>
public class RecordFloatCreditPurchaseRequest : RequestWithHostDateTime {
    /// <summary>
    /// Identifier of the float.
    /// </summary>
    public Guid FloatId { get; set; }

    /// <summary>
    /// Date and time of the credit purchase.
    /// </summary>
    public DateTimeOffset PurchaseDateTime { get; set; }

    /// <summary>
    /// Amount of credit added to the float.
    /// </summary>
    public Decimal CreditAmount { get; set; }
    /// <summary>
    /// Cost of purchasing the float credit.
    /// </summary>
    public Decimal CostPrice { get; set; }
}
