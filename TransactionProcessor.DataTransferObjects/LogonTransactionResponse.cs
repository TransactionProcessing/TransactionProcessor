namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    
    /// <summary>
    /// Processor response to a terminal logon transaction.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","responseCode":"VOUCHER-2026-001","responseMessage":"Example message","transactionId":"00000000-0000-0000-0000-000000000057","transactionType":"example"}</example>
    [ExcludeFromCodeCoverage]
    public class LogonTransactionResponse : TransactionResponse
    {
        
    }
}
