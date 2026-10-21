using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>
    /// Identifiers of the contract, estate, and newly added product.
    /// </summary>
    /// <example>{"contractId":"00000000-0000-0000-0000-000000000239","estateId":"00000000-0000-0000-0000-000000000023","productId":"00000000-0000-0000-0000-000000000146"}</example>
    [ExcludeFromCodeCoverage]
    public class AddProductToContractResponse
    {
        #region Properties
        /// <summary>The contract id.</summary>

        public Guid ContractId { get; set; }
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The product id.</summary>

        public Guid ProductId { get; set; }

        #endregion
    }
}
