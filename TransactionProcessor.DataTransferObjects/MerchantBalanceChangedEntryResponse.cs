using System;

namespace TransactionProcessor.DataTransferObjects
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// One merchant balance change with its event, timestamp, amount, reference, and resulting balance.
    /// </summary>
    /// <example>{"originalEventId":"00000000-0000-0000-0000-000000000234","estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","changeAmount":42.5,"dateTime":"2026-01-15T10:30:00Z","reference":"REF-2026-001","debitOrCredit":"example","balance":42.5}</example>
    [ExcludeFromCodeCoverage]
    public class MerchantBalanceChangedEntryResponse
    {
        /// <summary>The original event id.</summary>
        public Guid OriginalEventId { get; set; }
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The change amount.</summary>

        public Decimal ChangeAmount { get; set; }
        /// <summary>The date time.</summary>

        public DateTime DateTime { get; set; }
        /// <summary>The reference.</summary>

        public String Reference { get; set; }
        /// <summary>The debit or credit.</summary>

        public String DebitOrCredit { get; set; }
        /// <summary>The balance.</summary>

        public Decimal Balance { get; set; }
    }
}
