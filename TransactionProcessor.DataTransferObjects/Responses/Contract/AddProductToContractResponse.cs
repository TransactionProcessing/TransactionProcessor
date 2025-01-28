using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AddProductToContractResponse
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

        #endregion
    }
}