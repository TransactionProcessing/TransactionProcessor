namespace TransactionProcessor.BusinessLogic.OperatorInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OperatorResponse
    {
        #region Properties

        public Dictionary<String, String> AdditionalTransactionResponseMetadata { get; set; }

        public String AuthorisationCode { get; set; }
        
        public Boolean IsSuccessful { get; set; }

        public String ResponseCode { get; set; }

        public String ResponseMessage { get; set; }

        public String TransactionId { get; set; }

        #endregion
    }
}