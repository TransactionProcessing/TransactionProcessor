namespace TransactionProcessor.DataTransferObjects
{
    using System;
    
    public class IssueVoucherRequest
    {
        public String OperatorIdentifier { get; set; }

        public Guid EstateId { get; set; }

        public Guid TransactionId { get; set; }

        public Decimal Value { get; set; }

        public String RecipientEmail { get; set; }

        public String RecipientMobile { get; set; }

        public DateTime? IssuedDateTime { get; set; }
    }
}