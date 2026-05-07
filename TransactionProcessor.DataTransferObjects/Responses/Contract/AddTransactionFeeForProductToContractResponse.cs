using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    [ExcludeFromCodeCoverage]
    public class AddTransactionFeeForProductToContractResponse
    {
        #region Properties

        public Guid ContractId { get; set; }

        public Guid EstateId { get; set; }

        public Guid ProductId { get; set; }

        public Guid TransactionFeeId { get; set; }

        #endregion
    }
}