using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Settlement
{
    /// <summary>
    /// Settlement totals, completion status, and individual settlement fees.
    /// </summary>
    /// <example>{"isCompleted":false,"numberOfFeesSettled":1,"settlementDate":"2026-01-15T10:30:00Z","settlementFees":[{"calculatedValue":42.5,"feeDescription":"Example description","isSettled":false,"merchantId":"00000000-0000-0000-0000-000000000227","merchantName":"Example Name","settlementDate":"2026-01-15T10:30:00Z","settlementId":"00000000-0000-0000-0000-000000000215","transactionId":"00000000-0000-0000-0000-000000000057","operatorIdentifier":"EXAMPLE-001"}],"settlementId":"00000000-0000-0000-0000-000000000215","valueOfFeesSettled":42.5}</example>
    [ExcludeFromCodeCoverage]
    public class SettlementResponse
    {
        #region Constructors

        public SettlementResponse()
        {
            this.SettlementFees = new List<SettlementFeeResponse>();
        }

        #endregion

        #region Properties
        /// <summary>The is completed.</summary>

        public Boolean IsCompleted { get; set; }
        /// <summary>The number of fees settled.</summary>

        public Int32 NumberOfFeesSettled { get; set; }
        /// <summary>The settlement date.</summary>

        public DateTime SettlementDate { get; set; }
        /// <summary>The settlement fees.</summary>

        public List<SettlementFeeResponse> SettlementFees { get; set; }
        /// <summary>The settlement id.</summary>

        public Guid SettlementId { get; set; }
        /// <summary>The value of fees settled.</summary>

        public Decimal ValueOfFeesSettled { get; set; }

        #endregion
    }
}
