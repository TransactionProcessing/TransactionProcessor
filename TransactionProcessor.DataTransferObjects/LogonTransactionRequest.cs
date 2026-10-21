namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    
    /// <summary>
    /// Terminal logon message fields, including the shared transaction identity and timestamp.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","transactionDateTime":"2026-01-15T10:30:00Z","transactionType":"example","deviceIdentifier":"EXAMPLE-001","transactionNumber":"EXAMPLE-001"}</example>
    [ExcludeFromCodeCoverage]
    public class LogonTransactionRequest : TransactionRequest
    {
        #region Properties

        /// <summary>
        /// Identifier assigned to the terminal device submitting the logon.
        /// </summary>
        public String DeviceIdentifier { get; set; }
        
        /// <summary>
        /// Reference number assigned to the transaction by the terminal.
        /// </summary>
        public String TransactionNumber { get; set; }

        /// <summary>
        /// Transaction type reported by the terminal.
        /// </summary>
        public String TransactionType { get; set; }

        #endregion
    }
}
