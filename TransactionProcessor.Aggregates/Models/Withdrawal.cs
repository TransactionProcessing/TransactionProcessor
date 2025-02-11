using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Aggregates.Models
{
    [ExcludeFromCodeCoverage]
    internal record Withdrawal(Guid WithdrawalId,
                                     DateTime WithdrawalDateTime,
                                     Decimal Amount);
}