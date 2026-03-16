using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities;

[Table("merchantschedulemonth")]
public class MerchantScheduleMonth
{
    public Guid MerchantScheduleId { get; set; }

    public Int32 Month { get; set; }

    public String ClosedDays { get; set; } = String.Empty;
}
