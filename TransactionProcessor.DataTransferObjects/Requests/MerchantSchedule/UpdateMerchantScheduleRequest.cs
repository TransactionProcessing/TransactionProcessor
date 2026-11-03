using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.MerchantSchedule;

/// <summary>
/// Monthly closure-day definitions to apply to an existing merchant schedule.
/// </summary>
/// <example>{"months":[{"month":1,"closedDays":[]}]}</example>
[ExcludeFromCodeCoverage]
public class UpdateMerchantScheduleRequest
{
    /// <summary>
    /// Month entries that define closure days.
    /// </summary>
    public List<MerchantScheduleMonthRequest> Months { get; set; } = [];
}