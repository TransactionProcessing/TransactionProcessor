using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.MerchantSchedule
{
    /// <summary>
    /// Year and monthly closure-day definitions for a new merchant schedule.
    /// </summary>
    /// <example>{"year":1,"months":[{"month":1,"closedDays":[]}]}</example>
    [ExcludeFromCodeCoverage]
    public class CreateMerchantScheduleRequest
    {
        /// <summary>
        /// Calendar year for the schedule.
        /// </summary>
        public Int32 Year { get; set; }

        /// <summary>
        /// Month entries that define closure days.
        /// </summary>
        public List<MerchantScheduleMonthRequest> Months { get; set; } = [];
    }

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
}
