using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    public class ContractProductTransactionFee
    {
        #region Properties

        public CalculationType CalculationType { get; set; }

        public FeeType FeeType { get; set; }

        public String Description { get; set; }

        public Guid TransactionFeeId { get; set; }

        public Int32 TransactionFeeReportingId { get; set; }

        public Decimal Value { get; set; }

        #endregion
    }
}