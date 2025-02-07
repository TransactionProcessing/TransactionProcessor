using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Merchant
{
    public record Contact(Guid ContactId, String ContactEmailAddress, String ContactName, String ContactPhoneNumber);
}