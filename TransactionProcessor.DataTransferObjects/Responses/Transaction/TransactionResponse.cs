using System;

namespace TransactionProcessor.DataTransferObjects.Responses.Transaction
{
    /// <summary>
    /// Shared estate, merchant, response status, transaction identifier, and transaction type fields.
    /// </summary>
    /// <example>{"estateId":"00000000-0000-0000-0000-000000000023","merchantId":"00000000-0000-0000-0000-000000000227","responseCode":"VOUCHER-2026-001","responseMessage":"Example message","transactionId":"00000000-0000-0000-0000-000000000057","transactionType":"example"}</example>
    public class TransactionResponse {
        /// <summary>
        /// Estate that owns or scopes this operation.
        /// </summary>
        public Guid EstateId { get; set; }

        /// <summary>
        /// Merchant associated with the transaction.
        /// </summary>
        public Guid MerchantId { get; set; }
        /// <summary>The response code.</summary>

        public String ResponseCode { get; set; }
        /// <summary>The response message.</summary>

        public String ResponseMessage { get; set; }
        /// <summary>The transaction id.</summary>
        
        public Guid TransactionId { get; set; }
        /// <summary>
        /// Transaction type reported by the terminal.
        /// </summary>
        public String TransactionType { get; set; }
    }
}
