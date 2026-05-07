using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Merchant
{
    [ExcludeFromCodeCoverage]
    public class MerchantOperatorResponse
    {
        #region Properties
        public String Name { get; set; }

        public Guid OperatorId { get; set; }

        public String MerchantNumber { get; set; }

        public String TerminalNumber { get; set; }

        public Boolean IsDeleted { get; set; }

        #endregion
    }
}