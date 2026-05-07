using System;
using TransactionProcessor.DataTransferObjects.Responses.Contract;

namespace TransactionProcessor.DataTransferObjects.Requests.Contract
{
    /// <summary>
    /// 
    /// </summary>
    public class AddTransactionFeeForProductToContractRequest
    {
        #region Properties

        public CalculationType CalculationType { get; set; }

        public String Description { get; set; }

        public FeeType FeeType { get; set; }

        public Decimal Value { get; set; }

        #endregion
    }
}