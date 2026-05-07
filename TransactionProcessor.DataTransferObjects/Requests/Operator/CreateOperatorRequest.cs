using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Operator
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateOperatorRequest
    {
        #region Properties

        public String Name { get; set; }
        
        public Guid OperatorId { get; set; }

        public Boolean? RequireCustomMerchantNumber { get; set; }

        public Boolean? RequireCustomTerminalNumber { get; set; }

        #endregion
    }
}