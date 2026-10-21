using System;
using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.DataTransferObjects.Requests.Contract
{
    /// <summary>
    /// Fee calculation, category, description, and value to add to a contract product.
    /// </summary>
    /// <example>{"calculationType":0,"description":"Example description","feeType":0,"value":42.5}</example>
    public class AddTransactionFeeForProductToContractRequest
    {
        #region Properties

        /// <summary>
        /// Method used to calculate the fee value.
        /// </summary>
        public CalculationType CalculationType { get; set; }

        /// <summary>
        /// Human-readable description of the contract or fee.
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// Party to which the fee applies.
        /// </summary>
        public FeeType FeeType { get; set; }

        /// <summary>
        /// Monetary value associated with the product or fee.
        /// </summary>
        public Decimal Value { get; set; }

        #endregion
    }
}
