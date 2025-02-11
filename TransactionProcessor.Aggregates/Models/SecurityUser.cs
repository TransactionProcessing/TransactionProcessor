using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Aggregates.Models
{
    [ExcludeFromCodeCoverage]
    internal record SecurityUser(Guid SecurityUserId, String EmailAddress);
}
