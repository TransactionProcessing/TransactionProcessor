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
    public Int32 WithdrawalCount { get; set; }
    public Decimal TotalDeposited { get; set; }
    public Decimal TotalWithdrawn { get; set; }

    public Int32 SaleCount { get; set; }
    public Decimal AuthorisedSales { get; set; }
    public Decimal DeclinedSales { get; set; }

    public Int32 FeeCount { get; set; }
    public Decimal ValueOfFees { get; set; }

    public DateTime LastDeposit { get; set; }
    public DateTime LastWithdrawal { get; set; }
    public DateTime LastSale { get; set; }
    public DateTime LastFee { get; set; }

    public Int32 StartedTransactionCount { get; set; }
    public Int32 CompletedTransactionCount { get; set; }
}