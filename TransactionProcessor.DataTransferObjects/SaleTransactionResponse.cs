namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    
    /// <summary>
    /// Processor response to a sale transaction, including returned transaction metadata.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","responseCode":"VOUCHER-2026-001","responseMessage":"Example message","transactionId":"00000000-0000-0000-0000-000000000057","transactionType":"example","additionalTransactionMetadata":{"example":"example"}}</example>
    [ExcludeFromCodeCoverage]
    public class SaleTransactionResponse : TransactionResponse
    {
        /// <summary>Metadata returned with the sale transaction response.</summary>
        public Dictionary<String, String> AdditionalTransactionMetadata { get; set; }
    }
}
