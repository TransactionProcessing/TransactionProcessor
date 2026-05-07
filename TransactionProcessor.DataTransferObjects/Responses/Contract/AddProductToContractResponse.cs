using System;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Contract
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AddProductToContractResponse
    {
        #region Properties

        public Guid ContractId { get; set; }

        public Guid EstateId { get; set; }

        public Guid ProductId { get; set; }

        #endregion
    }
}