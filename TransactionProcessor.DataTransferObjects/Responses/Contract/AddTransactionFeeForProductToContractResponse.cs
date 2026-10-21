using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>Data transfer object for AddTransactionFeeForProductToContractResponse.</summary>
    /// <example>{"contractId":"00000000-0000-0000-0000-000000000239","estateId":"00000000-0000-0000-0000-000000000023","productId":"00000000-0000-0000-0000-000000000146","transactionFeeId":"00000000-0000-0000-0000-000000000074"}</example>
    [ExcludeFromCodeCoverage]
    public class AddTransactionFeeForProductToContractResponse
    {
        #region Properties
        /// <summary>The contract id.</summary>

        public Guid ContractId { get; set; }
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The product id.</summary>

        public Guid ProductId { get; set; }
        /// <summary>The transaction fee id.</summary>

        public Guid TransactionFeeId { get; set; }

        #endregion
    }
}
