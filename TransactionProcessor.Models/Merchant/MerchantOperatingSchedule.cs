using System;
using System.Collections.Generic;

namespace TransactionProcessor.Models.Merchant;

public class MerchantOperatingSchedule
{
    public Int32 Year { get; set; }

    public Boolean DefaultIsOpen { get; set; }

    public List<MerchantOperatingSchedulePeriod> Periods { get; set; } = new();
}
