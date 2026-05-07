namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    public class SaleTransactionRequest : TransactionRequest
    {
        #region Properties

        public Dictionary<String, String> AdditionalTransactionMetadata { get; set; }

        public Guid ContractId { get; set; }

        public String CustomerEmailAddress { get; set; }

        public String DeviceIdentifier { get; set; }

        public Guid OperatorId { get; set; }

        public Guid ProductId { get; set; }
        
        public String TransactionNumber { get; set; }

        public Int32? TransactionSource { get; set; }

        #endregion
    }
}