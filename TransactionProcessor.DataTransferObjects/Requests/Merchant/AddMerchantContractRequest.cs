using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant{
    [ExcludeFromCodeCoverage]
    public class AddMerchantContractRequest
    { 
        public Guid ContractId { get; set; }
    }
}