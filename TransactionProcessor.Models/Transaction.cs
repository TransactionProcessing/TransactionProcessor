namespace TransactionProcessor.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Transaction
    {
        #region Properties

        public Boolean IsAuthorised { get; set; }

        public Boolean IsComplete { get; set; }
        
        public Dictionary<String, String> AdditionalRequestMetadata { get; set; }

        public Dictionary<String, String> AdditionalResponseMetadata { get; set; }

        public String ResponseCode { get; set; }

        public String AuthorisationCode { get; set; }

        public Guid MerchantId { get; set; }

        public Guid OperatorId { get; set; }

        public String OperatorTransactionId { get; set; }

        public String ResponseMessage { get; set; }

        public Decimal TransactionAmount { get; set; }

        public DateTime TransactionDateTime { get; set; }

        public String TransactionNumber { get; set; }

        public String TransactionReference { get; set; }

        #endregion
    }
}