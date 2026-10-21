using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Operator
{
    /// <summary>
    /// Operator identity and configuration used to create an estate operator.
    /// </summary>
    /// <example>{"name":"Example Name","operatorId":"00000000-0000-0000-0000-000000000253","requireCustomMerchantNumber":false,"requireCustomTerminalNumber":false}</example>
    [ExcludeFromCodeCoverage]
    public class CreateOperatorRequest
    {
        #region Properties

        /// <summary>
        /// Display name for the estate, merchant, or operator.
        /// </summary>
        public String Name { get; set; }
        
        /// <summary>
        /// Operator associated with the operation.
        /// </summary>
        public Guid OperatorId { get; set; }

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
