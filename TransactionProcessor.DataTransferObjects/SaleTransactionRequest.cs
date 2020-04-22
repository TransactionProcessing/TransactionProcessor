using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.DataTransferObjects
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class SaleTransactionRequest : DataTransferObject
    {
        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        public String DeviceIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the transaction number.
        /// </summary>
        /// <value>
        /// The transaction number.
        /// </value>
        public String TransactionNumber { get; set; }

        /// <summary>
        /// Gets or sets the type of the transaction.
        /// </summary>
        /// <value>
        /// The type of the transaction.
        /// </value>
        public String TransactionType { get; set; }

        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        public String OperatorIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the customer email address.
        /// </summary>
        /// <value>
        /// The customer email address.
        /// </value>
        public String CustomerEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the additional transaction metadata.
        /// </summary>
        /// <value>
        /// The additional transaction metadata.
        /// </value>
        public Dictionary<String,String> AdditionalTransactionMetadata { get; set; }
    }
}
