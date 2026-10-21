using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant{
    /// <summary>
    /// Contract identifier to associate with a merchant.
    /// </summary>
    /// <example>{"contractId":"00000000-0000-0000-0000-000000000239"}</example>
    [ExcludeFromCodeCoverage]
    public class AddMerchantContractRequest
    { 
        /// <summary>
        /// Contract associated with the product or transaction.
        /// </summary>
        public Guid ContractId { get; set; }
    }
}
