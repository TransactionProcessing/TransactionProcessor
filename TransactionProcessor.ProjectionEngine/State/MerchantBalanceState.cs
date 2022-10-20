namespace TransactionProcessor.ProjectionEngine.State;

public record MerchantBalanceState : State
{
    public Guid EstateId { get; init; }
    public Guid MerchantId { get; init; }
    public String MerchantName { get; init; }
    public Decimal AvailableBalance { get; init; }
    public Decimal Balance { get; init; }

    public Int32 DepositCount { get; init; }
    public Decimal TotalDeposited { get; init; }

    public Int32 SaleCount { get; init; }
    public Decimal AuthorisedSales { get; init; }
    public Decimal DeclinedSales { get; init; }

    public Int32 FeeCount { get; init; }
    public Decimal ValueOfFees { get; init; }

    public DateTime LastDeposit { get; init; }
    public DateTime LastSale { get; init; }
    public DateTime LastFee { get; init; }

    public Int32 StartedTransactionCount { get; init; }
    public Int32 CompletedTransactionCount { get; init; }
}