namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class MerchantBalanceResponse
    {
        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public Decimal AvailableBalance { get; set; }

        public Decimal Balance { get; set; }
    }
}