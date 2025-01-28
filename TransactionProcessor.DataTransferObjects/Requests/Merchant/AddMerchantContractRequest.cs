using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant{
    [ExcludeFromCodeCoverage]
    public class AddMerchantContractRequest
    {
        [JsonProperty("contract_id")]
        public Guid ContractId { get; set; }
    }
}