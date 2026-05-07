using System;

namespace TransactionProcessor.DataTransferObjects
{
    public class GetVoucherResponse
    {
        public Guid VoucherId { get; set; }

        public DateTime ExpiryDate { get; set; }

        public Decimal Value { get; set; }

        public Decimal Balance { get; set; }

        public String VoucherCode { get; set; }

        public Boolean IsGenerated { get; set; }

        public Boolean IsIssued { get; set; }
        public Boolean IsRedeemed { get; set; }

        public DateTime IssuedDateTime { get; set; }

        public DateTime GeneratedDateTime { get; set; }
        
        public DateTime RedeemedDateTime { get; set; }
        
        public Guid TransactionId { get; set; }
    }
}
