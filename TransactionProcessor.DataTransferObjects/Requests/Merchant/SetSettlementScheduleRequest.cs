using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    [ExcludeFromCodeCoverage]
    public class SetSettlementScheduleRequest
    {
        public SettlementSchedule SettlementSchedule { get; set; }

    }
}
