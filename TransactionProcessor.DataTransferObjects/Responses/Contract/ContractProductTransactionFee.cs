using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>
    /// Transaction fee calculation details included in a contract product.
    /// </summary>
    /// <example>{"calculationType":0,"feeType":0,"description":"Example description","transactionFeeId":"00000000-0000-0000-0000-000000000074","transactionFeeReportingId":1,"value":42.5}</example>
    public class ContractProductTransactionFee
    {
        #region Properties
        /// <summary>The calculation type.</summary>

        public CalculationType CalculationType { get; set; }
        /// <summary>The fee type.</summary>

        public FeeType FeeType { get; set; }
        /// <summary>The description.</summary>

        public String Description { get; set; }
        /// <summary>The transaction fee id.</summary>

        public Guid TransactionFeeId { get; set; }
        /// <summary>The transaction fee reporting id.</summary>

        public Int32 TransactionFeeReportingId { get; set; }
        /// <summary>The value.</summary>

        public Decimal Value { get; set; }

        #endregion
    }
}
