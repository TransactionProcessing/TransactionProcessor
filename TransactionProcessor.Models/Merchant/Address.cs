using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Merchant
{
    public record Address(Guid AddressId, String AddressLine1, String AddressLine2, String AddressLine3, String AddressLine4, String Town, String Region, String PostalCode, String Country);
}