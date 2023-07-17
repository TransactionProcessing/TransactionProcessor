namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class SettlementResponse
    {
        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public DateTime SettlementDate { get; set; }

        public Int32 NumberOfFeesPendingSettlement { get; set; }

        public Int32 NumberOfFeesSettled { get; set; }

        public Boolean SettlementCompleted { get; set; }
    }
}