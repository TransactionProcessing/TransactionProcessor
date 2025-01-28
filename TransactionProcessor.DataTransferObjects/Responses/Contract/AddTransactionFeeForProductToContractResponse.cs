using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    [ExcludeFromCodeCoverage]
    public class AddTransactionFeeForProductToContractResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        /// <value>
        /// The contract identifier.
        /// </value>
        [JsonProperty("contract_id")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        [JsonProperty("product_id")]
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the transaction fee identifier.
        /// </summary>
        /// <value>
        /// The transaction fee identifier.
        /// </value>
        [JsonProperty("transaction_fee_id")]
        public Guid TransactionFeeId { get; set; }

        #endregion
    }
}