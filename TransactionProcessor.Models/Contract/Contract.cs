using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Contract
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Contract
    {
        #region Properties

        public Int32 ContractReportingId { get; set; }

        public Guid ContractId { get; set; }

        public String Description { get; set; }

        public Guid EstateId { get; set; }

        public Int32 EstateReportingId { get; set; }

        public Boolean IsCreated { get; set; }

        public Guid OperatorId { get; set; }

        public String OperatorName { get; set; }

        public List<Product> Products { get; set; }

        #endregion
    }
}