using System;

namespace TransactionProcessor.Models.Merchant
{
    public record Operator(Guid OperatorId, String Name, String MerchantNumber, String TerminalNumber, Boolean IsDeleted = false);
}