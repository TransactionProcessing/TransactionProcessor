using System;
using System.Collections.Generic;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant;

public class MerchantScheduleResponse
{
    public Int32 Year { get; set; }

    public List<MerchantScheduleMonthResponse> Months { get; set; } = [];
}

public class MerchantScheduleMonthResponse
{
    public Int32 Month { get; set; }

    public List<Int32> ClosedDays { get; set; } = [];
}
