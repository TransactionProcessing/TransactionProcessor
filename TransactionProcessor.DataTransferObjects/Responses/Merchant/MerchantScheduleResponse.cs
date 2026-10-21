using System;
using System.Collections.Generic;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant;

/// <summary>
/// Merchant schedule for a year, grouped by month and closed day.
/// </summary>
    /// <example>{"year":1,"months":[{"month":1,"closedDays":[]}]}</example>
public class MerchantScheduleResponse
{
        /// <summary>The year.</summary>
    public Int32 Year { get; set; }
        /// <summary>The months.</summary>

    public List<MerchantScheduleMonthResponse> Months { get; set; } = [];
}

/// <summary>
/// One schedule month and its closed day numbers.
/// </summary>
    /// <example>{"month":1,"closedDays":[]}</example>
public class MerchantScheduleMonthResponse
{
        /// <summary>The month.</summary>
    public Int32 Month { get; set; }
        /// <summary>The closed days.</summary>

    public List<Int32> ClosedDays { get; set; } = [];
}
