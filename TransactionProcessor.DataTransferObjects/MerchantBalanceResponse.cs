namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Stored and available balance values for a merchant.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","availableBalance":42.5,"balance":42.5}</example>
    [ExcludeFromCodeCoverage]
    public class MerchantBalanceResponse
    {
        /// <summary>The estate id.</summary>
        public Guid EstateId { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The available balance.</summary>

        public Decimal AvailableBalance { get; set; }
        /// <summary>The balance.</summary>

        public Decimal Balance { get; set; }
    }
}
