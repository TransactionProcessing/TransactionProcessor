using System;

namespace TransactionProcessor.Models.Merchant;

public record MerchantOperatingSchedulePeriod(DateTime StartDate, DateTime EndDate, Boolean IsOpen);
