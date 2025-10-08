using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using TransactionProcessor.Aggregates;
using TransactionProcessor.Models;

namespace TransactionProcessor.BusinessLogic.Common;

public static class TransactionHelpers {
    [ExcludeFromCodeCoverage]
    public static String GenerateTransactionReference()
    {
        Int64 i = 1;
        foreach (Byte b in Guid.NewGuid().ToByteArray())
        {
            i *= (b + 1);
        }

        return $"{i - DateTime.Now.Ticks:x}";
    }

    [ExcludeFromCodeCoverage]
    public static string GenerateAuthCode()
    {
        // 8 hex characters == 4 random bytes
        byte[] bytes = RandomNumberGenerator.GetBytes(4); // .NET 6+; for older frameworks use RandomNumberGenerator.Create().GetBytes(bytes)
        // Convert to uppercase hex string (0-9, A-F). Convert.ToHexString returns uppercase.
        string hex = Convert.ToHexString(bytes);
        return hex.Substring(0, 8);
    }

    public static DateTime CalculateSettlementDate(Models.Merchant.SettlementSchedule merchantSettlementSchedule,
                                                   DateTime completeDateTime)
    {
        if (merchantSettlementSchedule == Models.Merchant.SettlementSchedule.Weekly)
        {
            return completeDateTime.Date.AddDays(7).Date;
        }

        if (merchantSettlementSchedule == Models.Merchant.SettlementSchedule.Monthly)
        {
            return completeDateTime.Date.AddMonths(1).Date;
        }

        return completeDateTime.Date;
    }

    public static Boolean RequireFeeCalculation(TransactionAggregate transactionAggregate)
    {
        return transactionAggregate switch
        {
            _ when transactionAggregate.TransactionType == TransactionType.Logon => false,
            _ when transactionAggregate.IsAuthorised == false => false,
            _ when transactionAggregate.IsCompleted == false => false,
            _ when transactionAggregate.ContractId == Guid.Empty => false,
            _ when transactionAggregate.TransactionAmount == null => false,
            _ => true
        };
    }
}