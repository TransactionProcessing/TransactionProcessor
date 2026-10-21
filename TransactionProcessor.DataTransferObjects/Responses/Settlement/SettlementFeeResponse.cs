using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Settlement
{
    /// <summary>
    /// One fee included in a settlement, with merchant, transaction, operator, amount, and status.
    /// </summary>
    /// <example>{"calculatedValue":42.5,"feeDescription":"Example description","isSettled":false,"merchantId":"00000000-0000-0000-0000-000000000227","merchantName":"Example Name","settlementDate":"2026-01-15T10:30:00Z","settlementId":"00000000-0000-0000-0000-000000000215","transactionId":"00000000-0000-0000-0000-000000000057","operatorIdentifier":"EXAMPLE-001"}</example>
    [ExcludeFromCodeCoverage]
    public class SettlementFeeResponse
    {
        #region Properties
        /// <summary>The calculated value.</summary>
        public Decimal CalculatedValue { get; set; }
        /// <summary>The fee description.</summary>

        public String FeeDescription { get; set; }
        /// <summary>The is settled.</summary>

        public Boolean IsSettled { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The merchant name.</summary>

        public String MerchantName { get; set; }
        /// <summary>The settlement date.</summary>

        public DateTime SettlementDate { get; set; }
        /// <summary>The settlement id.</summary>

        public Guid SettlementId { get; set; }
        /// <summary>The transaction id.</summary>

        public Guid TransactionId { get; set; }
        /// <summary>The operator identifier.</summary>

        public String OperatorIdentifier { get; set; }

        #endregion
    }
}
