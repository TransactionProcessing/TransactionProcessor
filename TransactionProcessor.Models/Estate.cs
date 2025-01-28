using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Estate
    {
        #region Properties

        public Int32 EstateReportingId { get; set; }

        public Guid EstateId { get; set; }

        public String Name { get; set; }

        public String Reference { get; set; }

        public List<EstateOperator> Operators { get; set; }

        public List<SecurityUser> SecurityUsers { get; set; }

        #endregion
    }
}