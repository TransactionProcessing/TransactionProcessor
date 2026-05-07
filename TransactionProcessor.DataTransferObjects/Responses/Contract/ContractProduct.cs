using System;
using System.Collections.Generic;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    public class ContractProduct
    {
        public String DisplayText { get; set; }

        public String Name { get; set; }

        public Guid ProductId { get; set; }

        public Int32 ProductReportingId { get; set; }

        public List<ContractProductTransactionFee> TransactionFees { get; set; }

        public Decimal? Value { get; set; }

        public ProductType ProductType { get; set; }
    }
}