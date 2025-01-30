using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.Models.Estate
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Estate
    {
        #region Properties

        public int EstateReportingId { get; set; }

        public Guid EstateId { get; set; }

        public string Name { get; set; }

        public string Reference { get; set; }

        public List<Operator> Operators { get; set; }

        public List<SecurityUser> SecurityUsers { get; set; }

        #endregion
    }
}