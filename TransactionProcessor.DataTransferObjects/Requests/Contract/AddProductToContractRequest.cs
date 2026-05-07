using System;
using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.DataTransferObjects.Requests.Contract
{
    /// <summary>
    /// 
    /// </summary>
    public class AddProductToContractRequest
    {
        #region Properties

        public String DisplayText { get; set; }

        public String ProductName { get; set; }

        public Decimal? Value { get; set; }

        public ProductType ProductType { get; set; }

        #endregion
    }
}