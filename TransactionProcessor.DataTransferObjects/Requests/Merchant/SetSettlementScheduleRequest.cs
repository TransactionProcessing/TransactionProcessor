using System.Diagnostics.CodeAnalysis;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>Data transfer object for SetSettlementScheduleRequest.</summary>
    /// <example>{"settlementSchedule":0}</example>
    [ExcludeFromCodeCoverage]
    public class SetSettlementScheduleRequest
    {
        /// <summary>The settlement schedule.</summary>
        public SettlementSchedule SettlementSchedule { get; set; }

    }
}
