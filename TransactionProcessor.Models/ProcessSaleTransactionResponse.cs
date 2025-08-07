using System;
using System.Collections.Generic;

namespace TransactionProcessor.Models
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public class ProcessSaleTransactionResponse
    {
        #region Properties

        public String ResponseCode { get; set; }

        public String ResponseMessage { get; set; }

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public Dictionary<String, String> AdditionalTransactionMetadata { get; set; }

        public Guid TransactionId { get; set; }

        public Boolean TransactionIsComplete { get; set; }

        public Boolean TransactionIsAuthorised { get; set; }

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
