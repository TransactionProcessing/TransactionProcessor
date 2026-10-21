using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>Data transfer object for CreateContractResponse.</summary>
    /// <example>{"contractId":"00000000-0000-0000-0000-000000000239","estateId":"00000000-0000-0000-0000-000000000023","operatorId":"00000000-0000-0000-0000-000000000253"}</example>
    public class CreateContractResponse
    {
        #region Properties
        /// <summary>The contract id.</summary>
        public Guid ContractId { get; set; }
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The operator id.</summary>

        public Guid OperatorId { get; set; }

        #endregion
    }
}
