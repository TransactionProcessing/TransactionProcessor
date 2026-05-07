namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    public class SaleTransactionResponse : TransactionResponse
    {
        public Dictionary<String, String> AdditionalTransactionMetadata { get; set; }
    }
}