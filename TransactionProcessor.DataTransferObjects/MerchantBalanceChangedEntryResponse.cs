using System;

namespace TransactionProcessor.DataTransferObjects
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class MerchantBalanceChangedEntryResponse
    {
        public Guid OriginalEventId { get; set; }

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public Decimal ChangeAmount { get; set; }

        public DateTime DateTime { get; set; }

        public String Reference { get; set; }

        public String DebitOrCredit { get; set; }

        public Decimal Balance { get; set; }
    }
}
