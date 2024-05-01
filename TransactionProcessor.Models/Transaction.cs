﻿namespace TransactionProcessor.Models
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

        public Boolean IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the additional request metadata.
        /// </summary>
        /// <value>
        /// The additional request metadata.
        /// </value>
        public Dictionary<String, String> AdditionalRequestMetadata { get; set; }

        /// <summary>
        /// Gets or sets the additional response metadata.
        /// </summary>
        /// <value>
        /// The additional response metadata.
        /// </value>
        public Dictionary<String, String> AdditionalResponseMetadata { get; set; }

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public String ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the authorisation code.
        /// </summary>
        /// <value>
        /// The authorisation code.
        /// </value>
        public String AuthorisationCode { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        public Guid OperatorId { get; set; }

        /// <summary>
        /// Gets or sets the operator transaction identifier.
        /// </summary>
        /// <value>
        /// The operator transaction identifier.
        /// </value>
        public String OperatorTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets the transaction amount.
        /// </summary>
        /// <value>
        /// The transaction amount.
        /// </value>
        public Decimal TransactionAmount { get; set; }

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
        /// Gets or sets the transaction reference.
        /// </summary>
        /// <value>
        /// The transaction reference.
        /// </value>
        public String TransactionReference { get; set; }

        #endregion
    }
}