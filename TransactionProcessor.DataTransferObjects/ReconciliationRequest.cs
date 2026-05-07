namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    public class ReconciliationRequest : TransactionRequest
    {
        #region Properties

        public String DeviceIdentifier { get; set; }

        public List<OperatorTotalRequest> OperatorTotals { get; set; }

        public Int32 TransactionCount { get; set; }

        public Decimal TransactionValue { get; set; }

        #endregion
    }
}