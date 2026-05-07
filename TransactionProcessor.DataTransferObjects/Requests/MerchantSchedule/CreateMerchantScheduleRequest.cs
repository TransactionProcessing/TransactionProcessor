using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.MerchantSchedule
{
    [ExcludeFromCodeCoverage]
    public class CreateMerchantScheduleRequest
    {
        public Int32 Year { get; set; }

        public List<MerchantScheduleMonthRequest> Months { get; set; } = [];
    }

    [ExcludeFromCodeCoverage]
    public class UpdateMerchantScheduleRequest
    {
        public List<MerchantScheduleMonthRequest> Months { get; set; } = [];
    }

    [ExcludeFromCodeCoverage]
    public class MerchantScheduleMonthRequest
    {
        public Int32 Month { get; set; }

        public List<Int32> ClosedDays { get; set; } = [];
    }
}
