namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    public class LogonTransactionRequest : TransactionRequest
    {
        #region Properties

        public String DeviceIdentifier { get; set; }
        
        public String TransactionNumber { get; set; }

        public String TransactionType { get; set; }

        #endregion
    }
}