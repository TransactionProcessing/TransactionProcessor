using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Aggregates.Models
{
    [ExcludeFromCodeCoverage]
    public record SettledFee(Guid SettledFeeId, Guid TransactionId, DateTime DateTime, Decimal Amount);

    [ExcludeFromCodeCoverage]
    public record Transaction(Guid TransactionId, DateTime DateTime, Decimal Amount);

    
}