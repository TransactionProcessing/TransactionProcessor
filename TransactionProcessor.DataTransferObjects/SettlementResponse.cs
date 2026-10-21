
namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Pending and completed fee counts and status for a merchant settlement date.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","settlementDate":"2026-01-15T10:30:00Z","numberOfFeesPendingSettlement":1,"numberOfFeesSettled":1,"settlementCompleted":false}</example>
    [ExcludeFromCodeCoverage]
    public class SettlementResponse
    {
        /// <summary>The estate id.</summary>
        public Guid EstateId { get; set; }
        /// <summary>The merchant id.</summary>
        public Guid MerchantId { get; set; }
        /// <summary>The settlement date.</summary>

        public DateTime SettlementDate { get; set; }
        /// <summary>The number of fees pending settlement.</summary>

        public Int32 NumberOfFeesPendingSettlement { get; set; }
        /// <summary>The number of fees settled.</summary>
        public Int32 NumberOfFeesSettled { get; set; }
        /// <summary>The settlement completed.</summary>
        public Boolean SettlementCompleted { get; set; }
    }
}
