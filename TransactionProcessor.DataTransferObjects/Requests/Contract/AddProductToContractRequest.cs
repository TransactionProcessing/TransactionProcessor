using System;
using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.DataTransferObjects.Requests.Contract
{
    /// <summary>
    /// Product name, display details, type, and optional value to add to a contract.
    /// </summary>
    /// <example>{"displayText":"Example description","productName":"Example Name","value":42.5,"productType":0}</example>
    public class AddProductToContractRequest
    {
        #region Properties

        /// <summary>
        /// Text shown to a user for this product.
        /// </summary>
        public String DisplayText { get; set; }

        /// <summary>
        /// Name of the product.
        /// </summary>
        public String ProductName { get; set; }

        /// <summary>
        /// Monetary value associated with the product or fee.
        /// </summary>
        public Decimal? Value { get; set; }

        /// <summary>
        /// Category of product being added.
        /// </summary>
        public ProductType ProductType { get; set; }

        #endregion
    }
}
