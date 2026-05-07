using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Settlement
{
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

        public Boolean IsCompleted { get; set; }

        public Int32 NumberOfFeesSettled { get; set; }

        public DateTime SettlementDate { get; set; }

        public List<SettlementFeeResponse> SettlementFees { get; set; }

        public Guid SettlementId { get; set; }

        public Decimal ValueOfFeesSettled { get; set; }

        #endregion
    }
}