using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant{
    [ExcludeFromCodeCoverage]
    public class MerchantContractResponse
    {
        public Guid ContractId { get; set; }
        public Boolean IsDeleted { get; set; }

        public List<Guid> ContractProducts { get; set; }

        public MerchantContractResponse()
        {
            this.ContractProducts = new List<Guid>();
        }
    }
}