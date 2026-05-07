using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AssignOperatorResponse
    {
        #region Properties

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public Guid OperatorId { get; set; }

        #endregion
    }
}