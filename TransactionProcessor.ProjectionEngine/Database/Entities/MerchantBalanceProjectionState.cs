namespace TransactionProcessor.ProjectionEngine.Database.Entities;

using System.ComponentModel.DataAnnotations;

public class MerchantBalanceProjectionState
{
    [Timestamp]
    public Byte[] Timestamp { get; set; }
    public Guid EstateId { get; set; }
    public Guid MerchantId { get; set; }
    public String MerchantName { get; set; }
    public Decimal AvailableBalance { get; set; }
    public Decimal Balance { get; init; }
    public Int32 DepositCount { get; set; }
    public Decimal TotalDeposited { get; set; }

    public Int32 SaleCount { get; set; }
    public Decimal AuthorisedSales { get; set; }
    public Decimal DeclinedSales { get; set; }

    public Int32 FeeCount { get; set; }
    public Decimal ValueOfFees { get; set; }

    public DateTime LastDeposit { get; set; }
    public DateTime LastSale { get; set; }
    public DateTime LastFee { get; set; }

    public Int32 StartedTransactionCount { get; set; }
    public Int32 CompletedTransactionCount { get; set; }
}

public class MerchantBalanceChangedEntry
{
    public Guid AggregateId { get; set; }
    public Guid OriginalEventId { get; set; }
    public Guid EstateId { get; set; }
    public Guid MerchantId { get; set; }
    public Decimal Balance { get; set; }
    public Decimal ChangeAmount { get; set; }
    public DateTime DateTime { get; set; }
    public String Reference { get; set; }
    public Guid CauseOfChangeId { get; set; }
    public String DebitOrCredit { get; set; }
}
