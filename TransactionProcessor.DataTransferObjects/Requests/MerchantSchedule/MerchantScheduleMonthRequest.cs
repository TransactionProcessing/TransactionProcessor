using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.MerchantSchedule;

/// <summary>
/// One schedule month and the day numbers on which the merchant is closed.
/// </summary>
/// <example>{"month":1,"closedDays":[]}</example>
[ExcludeFromCodeCoverage]
public class MerchantScheduleMonthRequest
{
    /// <summary>
    /// Calendar month number.
    /// </summary>
    public Int32 Month { get; set; }

    /// <summary>
    /// Day numbers in the month when the merchant is closed.
    /// </summary>
    public List<Int32> ClosedDays { get; set; } = [];
}