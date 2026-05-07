namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    public class TransactionRequest
    {
        #region Properties

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public DateTime TransactionDateTime { get; set; }

        public String TransactionType { get; set; }

        #endregion
    }

    public class TransactionResponse {
        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public String ResponseCode { get; set; }

        public String ResponseMessage { get; set; }
        
        public Guid TransactionId { get; set; }
        public String TransactionType { get; set; }
    }
}