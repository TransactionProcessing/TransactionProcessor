using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    public class GenerateMerchantStatementResponse
    {
        public Guid MerchantStatementId { get; set; }
        public Guid MerchantId { get; set; }
        public Guid EstateId { get; set; }
    }
}