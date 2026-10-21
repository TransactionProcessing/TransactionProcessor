using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    /// <summary>
    /// Identifiers of the estate, merchant, and assigned operator.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","operatorId":"00000000-0000-0000-0000-000000000253"}</example>
    [ExcludeFromCodeCoverage]
    public class AssignOperatorResponse
    {
        #region Properties
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The merchant id.</summary>

        public Guid MerchantId { get; set; }
        /// <summary>The operator id.</summary>

        public Guid OperatorId { get; set; }

        #endregion
    }
}
