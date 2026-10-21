using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Operator{
    /// <summary>
    /// Operator name and optional merchant or terminal number requirements to update.
    /// </summary>
    /// <example>{"name":"Example Name","requireCustomMerchantNumber":false,"requireCustomTerminalNumber":false}</example>
    [ExcludeFromCodeCoverage]
    public class UpdateOperatorRequest
    {
        #region Properties

        /// <summary>
        /// Display name for the estate, merchant, or operator.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Whether a custom merchant number is required for this operator.
        /// </summary>
        public Boolean? RequireCustomMerchantNumber { get; set; }

        /// <summary>
        /// Whether a custom terminal number is required for this operator.
        /// </summary>
        public Boolean? RequireCustomTerminalNumber { get; set; }

        #endregion
    }
}
