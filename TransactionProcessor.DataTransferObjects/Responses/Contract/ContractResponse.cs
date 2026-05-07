using System;
using System.Collections.Generic;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    public class ContractResponse
    {
        public Guid ContractId { get; set; }

        public Int32 ContractReportingId { get; set; }

        public String Description { get; set; }

        public Guid EstateId { get; set; }

        public Int32 EstateReportingId { get; set; }

        public Guid OperatorId { get; set; }

        public String OperatorName { get; set; }

        public List<ContractProduct> Products { get; set; }
    }
}
