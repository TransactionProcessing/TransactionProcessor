namespace TransactionProcessor.DataTransferObjects
{
    using System;

    public class PendingSettlementResponse
    {
        public Guid EstateId { get; set; }

        public DateTime SettlementDate { get; set; }

        public Int32 NumberOfFeesPendingSettlement { get; set; }

        public Int32 NumberOfFeesSettled { get; set; }
        
    }
}