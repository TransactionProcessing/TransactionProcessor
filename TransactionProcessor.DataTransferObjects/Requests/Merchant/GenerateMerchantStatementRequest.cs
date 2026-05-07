using System;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    public class GenerateMerchantStatementRequest
    {
        #region Properties

        public DateTime MerchantStatementDate { get; set; }

        #endregion
    }
}