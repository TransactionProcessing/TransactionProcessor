namespace TransactionProcessor.ProjectionEngine.State;

public record MerchantBalanceState : State
{
    public Guid EstateId { get; init; }
    public Guid MerchantId { get; init; }
    public String MerchantName { get; init; }
    public Decimal AvailableBalance { get; init; }
    public Decimal Balance { get; init; }

    public Int32 DepositCount { get; init; }

    public Int32 WithdrawalCount { get; init; }
    public Decimal TotalDeposited { get; init; }
    public Decimal TotalWithdrawn { get; init; }

    public Int32 SaleCount { get; init; }
    public Decimal AuthorisedSales { get; init; }
    public Decimal DeclinedSales { get; init; }

    public Int32 FeeCount { get; init; }
    public Decimal ValueOfFees { get; init; }

    public DateTime LastDeposit { get; init; }
    public DateTime LastWithdrawal { get; init; }
    public DateTime LastSale { get; init; }
    public DateTime LastFee { get; init; }

    public Int32 StartedTransactionCount { get; init; }
    public Int32 CompletedTransactionCount { get; init; }
}

public record AuthorisedSales(int count, decimal value, DateTime? lastSale);

public record DeclinedSales(int count, decimal value, DateTime? lastSale);

public record Deposits(int count, decimal value, DateTime? lastDeposit);

public record Fees(int count, decimal value);

public record Merchant(string Id, string Name, int numberOfEventsProcessed, decimal balance, 
                       Deposits deposits, Withdrawals withdrawals, AuthorisedSales authorisedSales, 
                       DeclinedSales declinedSales, Fees fees);

public record MerchantBalanceProjectionState1(Merchant merchant);

public record Withdrawals(int count, decimal value, DateTime? lastWithdrawal);

