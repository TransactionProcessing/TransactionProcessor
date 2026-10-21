using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant{
    /// <summary>Data transfer object for MerchantContractResponse.</summary>
    /// <example>{"contractId":"00000000-0000-0000-0000-000000000239","isDeleted":false,"contractProducts":[]}</example>
    [ExcludeFromCodeCoverage]
    public class MerchantContractResponse
    {
        /// <summary>The contract id.</summary>
        public Guid ContractId { get; set; }
        /// <summary>The is deleted.</summary>
        public Boolean IsDeleted { get; set; }
        /// <summary>The contract products.</summary>

        public List<Guid> ContractProducts { get; set; }

        public MerchantContractResponse()
        {
            this.ContractProducts = new List<Guid>();
        }
    }
}
