namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    
    /// <summary>
    /// Sale details including merchant, device, operator, contract, product, and optional metadata.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","transactionDateTime":"2026-01-15T10:30:00Z","transactionType":"example","contractId":"00000000-0000-0000-0000-000000000239","customerEmailAddress":"customer@example.com","deviceIdentifier":"EXAMPLE-001","operatorId":"00000000-0000-0000-0000-000000000253","productId":"00000000-0000-0000-0000-000000000146","transactionNumber":"EXAMPLE-001","transactionSource":1,"additionalTransactionMetadata":{"channel":"terminal"}}</example>
    [ExcludeFromCodeCoverage]
    public class SaleTransactionRequest : TransactionRequest
    {
        #region Properties

        /// <summary>
        /// Optional key/value metadata to include with the transaction.
        /// </summary>
        public Dictionary<String, String> AdditionalTransactionMetadata { get; set; }

        /// <summary>
        /// Contract associated with the product or transaction.
        /// </summary>
        public Guid ContractId { get; set; }

        /// <summary>
        /// Optional customer email address associated with the sale.
        /// </summary>
        public String CustomerEmailAddress { get; set; }

        /// <summary>
        /// Identifier assigned to the terminal device submitting the sale.
        /// </summary>
        public String DeviceIdentifier { get; set; }

        /// <summary>
        /// Operator associated with the operation.
        /// </summary>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Product associated with the contract or transaction.
        /// </summary>
        public Guid ProductId { get; set; }
        
        /// <summary>
        /// Reference number assigned to the transaction by the terminal.
        /// </summary>
        public String TransactionNumber { get; set; }

        /// <summary>
        /// Numeric transaction source code; omitted values default to online source code 1.
        /// </summary>
        public Int32? TransactionSource { get; set; }

        #endregion
    }
}
