using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>Data transfer object for MerchantOperatorResponse.</summary>
    /// <example>{"name":"Example Name","operatorId":"00000000-0000-0000-0000-000000000253","merchantNumber":"EXAMPLE-001","terminalNumber":"EXAMPLE-001","isDeleted":false}</example>
    [ExcludeFromCodeCoverage]
    public class MerchantOperatorResponse
    {
        #region Properties
        /// <summary>The name.</summary>
        public String Name { get; set; }
        /// <summary>The operator id.</summary>

        public Guid OperatorId { get; set; }
        /// <summary>The merchant number.</summary>

        public String MerchantNumber { get; set; }
        /// <summary>The terminal number.</summary>

        public String TerminalNumber { get; set; }
        /// <summary>The is deleted.</summary>

        public Boolean IsDeleted { get; set; }

        #endregion
    }
}
