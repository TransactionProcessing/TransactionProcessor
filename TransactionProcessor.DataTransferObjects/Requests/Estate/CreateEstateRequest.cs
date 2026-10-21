using System;
using System.ComponentModel.DataAnnotations;

namespace TransactionProcessor.DataTransferObjects.Requests.Estate
{
    /// <summary>
    /// Estate identifier and display name used to create an estate.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","estateName":"Example Name"}</example>
    public class CreateEstateRequest
    {
        /// <summary>
        /// Estate that owns or scopes this operation.
        /// </summary>
        public Guid EstateId { get; set; }

        /// <summary>
        /// Display name for the estate.
        /// </summary>
        public String EstateName { get; set; }
    }
}
