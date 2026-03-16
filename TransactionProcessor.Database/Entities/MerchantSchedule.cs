using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransactionProcessor.Database.Entities;

[Table("merchantschedule")]
public class MerchantSchedule
{
    public Guid MerchantScheduleId { get; set; }

    public Guid EstateId { get; set; }

    public Guid MerchantId { get; set; }

    public Int32 Year { get; set; }
}
