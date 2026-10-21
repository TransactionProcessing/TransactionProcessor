namespace TransactionProcessor.DataTransferObjects
{
    using System;
    /// <summary>Data transfer object for IssueVoucherRequest.</summary>
    /// <example>{"operatorIdentifier":"EXAMPLE-001","estateId":"00000000-0000-0000-0000-000000000023","transactionId":"00000000-0000-0000-0000-000000000057","value":42.5,"recipientEmail":"customer@example.com","recipientMobile":"+15550100","issuedDateTime":"2026-01-15T10:30:00Z"}</example>
    
    public class IssueVoucherRequest
    {
        /// <summary>The operator identifier.</summary>
        public String OperatorIdentifier { get; set; }
        /// <summary>The estate id.</summary>

        public Guid EstateId { get; set; }
        /// <summary>The transaction id.</summary>

        public Guid TransactionId { get; set; }
        /// <summary>The value.</summary>

        public Decimal Value { get; set; }
        /// <summary>The recipient email.</summary>

        public String RecipientEmail { get; set; }
        /// <summary>The recipient mobile.</summary>

        public String RecipientMobile { get; set; }
        /// <summary>The issued date time.</summary>

        public DateTime? IssuedDateTime { get; set; }
    }
}
