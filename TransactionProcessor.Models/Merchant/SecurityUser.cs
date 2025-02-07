using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Merchant
{
    public record SecurityUser(Guid SecurityUserId, String EmailAddress);
}
