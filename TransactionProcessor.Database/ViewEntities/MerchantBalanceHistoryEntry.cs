namespace TransactionProcessor.ProjectionEngine.Database.Database.ViewEntities
{
    using System;

    public class MerchantBalanceHistoryViewEntry
    {
        public Guid OriginalEventId { get; set; }

        public Decimal ChangeAmount { get; set; }

        public DateTime EntryDateTime { get; set; }

        public String Reference { get; set; }

        public String DebitOrCredit { get; set; }

        public Guid MerchantId { get; set; }

        public Decimal Balance { get; set; }
    }
}
