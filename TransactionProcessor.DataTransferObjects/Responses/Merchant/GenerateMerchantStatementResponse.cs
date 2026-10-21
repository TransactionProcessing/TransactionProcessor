using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Data transfer object for GenerateMerchantStatementResponse.</summary>
    /// <example>{"merchantStatementId":"00000000-0000-0000-0000-000000000156","merchantId":"00000000-0000-0000-0000-000000000227","estateId":"00000000-0000-0000-0000-000000000023"}</example>
    public class GenerateMerchantStatementResponse
    {
        /// <summary>The merchant statement id.</summary>
        public Guid MerchantStatementId { get; set; }
        /// <summary>The merchant id.</summary>
        public Guid MerchantId { get; set; }
        /// <summary>The estate id.</summary>
        public Guid EstateId { get; set; }
    }
}
