using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Operator
{
    /// <summary>
    /// Operator identity and merchant or terminal number configuration.
    /// </summary>
    /// <example>{"name":"Example Name","operatorId":"00000000-0000-0000-0000-000000000253","requireCustomMerchantNumber":false,"requireCustomTerminalNumber":false}</example>
    public class OperatorResponse
    {
        /// <summary>The name.</summary>
        public String Name { get; set; }
        /// <summary>The operator id.</summary>

        public Guid OperatorId { get; set; }
        /// <summary>The require custom merchant number.</summary>

        public Boolean RequireCustomMerchantNumber { get; set; }
        /// <summary>The require custom terminal number.</summary>

        public Boolean RequireCustomTerminalNumber { get; set; }


    }
}
