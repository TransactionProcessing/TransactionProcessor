using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EstateResponse
    {
        #region Properties

        public Guid EstateId { get; set; }

        public Int32 EstateReportingId { get; set; }

        public String EstateName { get; set; }

        public String EstateReference { get; set; }

        public List<EstateOperatorResponse> Operators { get; set; }
        
        public List<SecurityUserResponse> SecurityUsers { get; set; }

        #endregion
    }
}