using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant{
    [ExcludeFromCodeCoverage]
    public class MerchantContractResponse
    {
        [JsonProperty("contract_id")]
        public Guid ContractId { get; set; }

        [JsonProperty("is_deleted")]
        public Boolean IsDeleted { get; set; }

        [JsonProperty("contract_products")]
        public List<Guid> ContractProducts { get; set; }

        public MerchantContractResponse()
        {
            this.ContractProducts = new List<Guid>();
        }
    }
}