using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Aggregates.Models
{
    [ExcludeFromCodeCoverage]
    internal record Operator(Guid OperatorId, String Name, String MerchantNumber, String TerminalNumber, Boolean IsDeleted = false);
}