using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Contract
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Product
    {
        #region Constructors

        public Product() {
            this.TransactionFees = new List<ContractProductTransactionFee>();
        }

        #endregion

        #region Properties

        public String DisplayText { get; set; }

        public String Name { get; set; }

        public Guid ContractProductId { get; set; }

        public Int32 ContractProductReportingId { get; set; }

        public ProductType ProductType { get; set; }

        public List<ContractProductTransactionFee> TransactionFees { get; set; }

        public Decimal? Value { get; set; }

        #endregion
    }
}