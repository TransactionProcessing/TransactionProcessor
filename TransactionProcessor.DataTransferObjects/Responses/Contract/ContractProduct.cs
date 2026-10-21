using System;
using System.Collections.Generic;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>
    /// Product details and transaction fees included in a contract.
    /// </summary>
    /// <example>{"displayText":"Example description","name":"Example Name","productId":"00000000-0000-0000-0000-000000000146","productReportingId":1,"transactionFees":[{"calculationType":0,"feeType":0,"description":"Example description","transactionFeeId":"00000000-0000-0000-0000-000000000074","transactionFeeReportingId":1,"value":42.5}],"value":42.5,"productType":0}</example>
    public class ContractProduct
    {
        /// <summary>The display text.</summary>
        public String DisplayText { get; set; }
        /// <summary>The name.</summary>

        public String Name { get; set; }
        /// <summary>The product id.</summary>

        public Guid ProductId { get; set; }
        /// <summary>The product reporting id.</summary>

        public Int32 ProductReportingId { get; set; }
        /// <summary>The transaction fees.</summary>

        public List<ContractProductTransactionFee> TransactionFees { get; set; }
        /// <summary>The value.</summary>

        public Decimal? Value { get; set; }
        /// <summary>The product type.</summary>

        public ProductType ProductType { get; set; }
    }
}
