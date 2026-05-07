using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Operator{
    [ExcludeFromCodeCoverage]
    public class UpdateOperatorRequest
    {
        #region Properties

        public String Name { get; set; }

        public Boolean? RequireCustomMerchantNumber { get; set; }

        public Boolean? RequireCustomTerminalNumber { get; set; }

        #endregion
    }
}