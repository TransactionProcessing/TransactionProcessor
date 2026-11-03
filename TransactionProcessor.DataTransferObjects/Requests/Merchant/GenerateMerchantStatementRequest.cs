using System;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// Statement date for the merchant statement to generate.
    /// </summary>
    /// <example>{"merchantStatementDate":"2026-01-15T10:30:00Z"}</example>
    public class GenerateMerchantStatementRequest
    {
        /// <summary>
        /// Statement date for the statement to generate.
        /// </summary>
        public DateTimeOffset MerchantStatementDate { get; set; }
    }
}
