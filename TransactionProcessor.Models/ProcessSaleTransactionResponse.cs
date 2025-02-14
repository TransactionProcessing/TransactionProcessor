using System;
using System.Collections.Generic;

namespace TransactionProcessor.Models
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class ProcessSaleTransactionResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public String ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; set; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the additional transaction metadata.
        /// </summary>
        /// <value>
        /// The additional transaction metadata.
        /// </value>
        public Dictionary<String, String> AdditionalTransactionMetadata { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public Guid TransactionId { get; set; }

        public Boolean TransactionIsComplete { get; set; }

        #endregion
    }

    [ExcludeFromCodeCoverage]
    public class ProcessSettlementResponse
    {
        public Int32 NumberOfFeesPendingSettlement { get; set; }

        public Int32 NumberOfFeesSuccessfullySettled { get; set; }

        public Int32 NumberOfFeesFailedToSettle { get; set; }
    }
}
