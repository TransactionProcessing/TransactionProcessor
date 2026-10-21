using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>
    /// Estate details with its operators and security users.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","estateReportingId":1,"estateName":"Example Name","estateReference":"REF-2026-001","operators":[{"name":"Example Name","operatorId":"00000000-0000-0000-0000-000000000253","requireCustomMerchantNumber":false,"requireCustomTerminalNumber":false,"isDeleted":false}],"securityUsers":[{"emailAddress":"customer@example.com","securityUserId":"00000000-0000-0000-0000-000000000170"}]}</example>
    [ExcludeFromCodeCoverage]
    public class EstateResponse
    {
        #region Properties
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The estate reporting id.</summary>

        public Int32 EstateReportingId { get; set; }
        /// <summary>The estate name.</summary>

        public String EstateName { get; set; }
        /// <summary>The estate reference.</summary>

        public String EstateReference { get; set; }
        /// <summary>The operators.</summary>

        public List<EstateOperatorResponse> Operators { get; set; }
        /// <summary>The security users.</summary>
        
        public List<SecurityUserResponse> SecurityUsers { get; set; }

        #endregion
    }
}
