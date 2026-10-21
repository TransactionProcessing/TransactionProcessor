using System;
using System.Collections.Generic;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>
    /// Contract identity and details, including products and their transaction fees.
    /// </summary>
    /// <example>{"contractId":"00000000-0000-0000-0000-000000000239","contractReportingId":1,"description":"Example description","estateId":"00000000-0000-0000-0000-000000000023","estateReportingId":1,"operatorId":"00000000-0000-0000-0000-000000000253","operatorName":"Example Name","products":[{"displayText":"Example description","name":"Example Name","productId":"00000000-0000-0000-0000-000000000146","productReportingId":1,"transactionFees":[{"calculationType":0,"feeType":0,"description":"Example description","transactionFeeId":"00000000-0000-0000-0000-000000000074","transactionFeeReportingId":1,"value":42.5}],"value":42.5,"productType":0}]}</example>
    public class ContractResponse
    {
        /// <summary>The contract id.</summary>
        public Guid ContractId { get; set; }
        /// <summary>The contract reporting id.</summary>

        public Int32 ContractReportingId { get; set; }
        /// <summary>The description.</summary>

        public String Description { get; set; }
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The estate reporting id.</summary>

        public Int32 EstateReportingId { get; set; }
        /// <summary>The operator id.</summary>

        public Guid OperatorId { get; set; }
        /// <summary>The operator name.</summary>

        public String OperatorName { get; set; }
        /// <summary>The products.</summary>

        public List<ContractProduct> Products { get; set; }
    }
}
