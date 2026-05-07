using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    public class CreateContractResponse
    {
        #region Properties
        public Guid ContractId { get; set; }

        public Guid EstateId { get; set; }

        public Guid OperatorId { get; set; }

        #endregion
    }
}