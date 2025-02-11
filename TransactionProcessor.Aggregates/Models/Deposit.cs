using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.Models.Merchant;

namespace TransactionProcessor.Aggregates.Models
{
    [ExcludeFromCodeCoverage]
    internal record Deposit(Guid DepositId,
                            MerchantDepositSource Source,
                            String Reference,
                            DateTime DepositDateTime,
                            Decimal Amount);
}