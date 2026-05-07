namespace TransactionProcessor.DataTransferObjects
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    
    [ExcludeFromCodeCoverage]
    public class OperatorTotalRequest
    {
        #region Properties

        public Guid ContractId { get; set; }

        public Guid OperatorId { get; set; }

        public Int32 TransactionCount { get; set; }

        public Decimal TransactionValue { get; set; }

        #endregion
    }
}