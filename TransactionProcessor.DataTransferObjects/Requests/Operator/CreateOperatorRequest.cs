using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Operator
{
    /// <summary>
    /// Operator identity and configuration used to create an estate operator.
    /// </summary>
    /// <example>{"name":"Example Name","requireCustomMerchantNumber":false,"requireCustomTerminalNumber":false}</example>
    [ExcludeFromCodeCoverage]
    public class CreateOperatorRequest
    {
        /// <summary>
        /// Display name for the operator.
        /// </summary>
        [Required]
        public String Name { get; set; }
        
        /// <summary>
        /// Whether a custom merchant number is required for this operator.
        /// </summary>
        public Boolean? RequireCustomMerchantNumber { get; set; }

        /// <summary>
        /// Whether a custom terminal number is required for this operator.
        /// </summary>
        public Boolean? RequireCustomTerminalNumber { get; set; }
    }
}
