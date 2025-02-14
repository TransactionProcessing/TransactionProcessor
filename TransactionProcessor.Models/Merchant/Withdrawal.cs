using System;

namespace TransactionProcessor.Models.Merchant;

public class Withdrawal
{
    #region Properties
        
    public Decimal Amount { get; set; }
        
    public DateTime WithdrawalDateTime { get; set; }
        
    public Guid WithdrawalId { get; set; }

    #endregion
}