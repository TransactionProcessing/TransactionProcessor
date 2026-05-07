using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Requests.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AssignOperatorRequest
    {
        #region Properties

        public String MerchantNumber { get; set; }

        public Guid OperatorId { get; set; }

        public String TerminalNumber { get; set; }

        #endregion
    }
}