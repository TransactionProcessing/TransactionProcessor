using System;

namespace TransactionProcessor.DataTransferObjects.Responses.File
{
    /// <summary>Data transfer object for TransactionResponse.</summary>
    /// <example>{"transactionId":"00000000-0000-0000-0000-000000000057","authCode":"VOUCHER-2026-001","isAuthorised":false,"isCompleted":false,"responseCode":"VOUCHER-2026-001","responseMessage":"Example message","transactionNumber":"EXAMPLE-001"}</example>
    public class TransactionResponse
    {
        /// <summary>The transaction id.</summary>
        public Guid TransactionId { get; set; }
        /// <summary>The auth code.</summary>
        public String AuthCode { get; set; }
        /// <summary>The is authorised.</summary>
        public Boolean IsAuthorised { get; set; }
        /// <summary>The is completed.</summary>
        public Boolean IsCompleted { get; set; }
        /// <summary>The response code.</summary>
        public String ResponseCode { get; set; }
        /// <summary>The response message.</summary>
        public String ResponseMessage { get; set; }
        /// <summary>The transaction number.</summary>
        public String TransactionNumber { get; set; }
    }
}
