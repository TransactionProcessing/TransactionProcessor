namespace TransactionProcessor.Aggregates.Models
{
    internal record Operator(Guid OperatorId, String Name, String MerchantNumber, String TerminalNumber, Boolean IsDeleted = false);
}