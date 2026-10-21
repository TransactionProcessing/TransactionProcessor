using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>Data transfer object for EstateOperatorResponse.</summary>
    /// <example>{"name":"Example Name","operatorId":"00000000-0000-0000-0000-000000000253","requireCustomMerchantNumber":false,"requireCustomTerminalNumber":false,"isDeleted":false}</example>
    [ExcludeFromCodeCoverage]
    public class EstateOperatorResponse
    {
        #region Properties
        /// <summary>The name.</summary>

        public String Name { get; set; }
        /// <summary>The operator id.</summary>

        public Guid OperatorId { get; set; }
        /// <summary>The require custom merchant number.</summary>

        public Boolean RequireCustomMerchantNumber { get; set; }
        /// <summary>The require custom terminal number.</summary>

        public Boolean RequireCustomTerminalNumber { get; set; }
        /// <summary>The is deleted.</summary>

        public Boolean IsDeleted { get; set; }

        #endregion
    }
}
