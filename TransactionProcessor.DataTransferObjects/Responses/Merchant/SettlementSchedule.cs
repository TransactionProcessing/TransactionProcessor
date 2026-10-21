namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Schedules that determine when merchant fees are settled.</summary>
    /// <example>0</example>
    public enum SettlementSchedule
    {
        /// <summary>No settlement schedule has been selected.</summary>
        NotSet,
        /// <summary>Settle fees as soon as they are eligible.</summary>
        Immediate,
        /// <summary>Settle fees once each week.</summary>
        Weekly,
        /// <summary>Settle fees once each month.</summary>
        Monthly
    }
}
