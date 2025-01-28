using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace TransactionProcessor.DataTransferObjects.Responses.Estate
{
    /// <summary>
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EstateResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        [JsonProperty("estate_reporting_id")]
        public Int32 EstateReportingId { get; set; }

        /// <summary>
        /// Gets or sets the name of the estate.
        /// </summary>
        /// <value>
        /// The name of the estate.
        /// </value>
        [JsonProperty("estate_name")]
        public String EstateName { get; set; }

        [JsonProperty("estate_reference")]
        public String EstateReference { get; set; }

        /// <summary>
        /// Gets or sets the operators.
        /// </summary>
        /// <value>
        /// The operators.
        /// </value>
        [JsonProperty("operators")]
        public List<EstateOperatorResponse> Operators { get; set; }

        /// <summary>
        /// Gets or sets the security users.
        /// </summary>
        /// <value>
        /// The security users.
        /// </value>
        [JsonProperty("security_users")]
        public List<SecurityUserResponse> SecurityUsers { get; set; }

        #endregion
    }
}