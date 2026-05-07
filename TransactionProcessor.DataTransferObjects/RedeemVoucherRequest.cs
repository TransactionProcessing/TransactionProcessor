namespace TransactionProcessor.DataTransferObjects
{
    using System;

    public class RedeemVoucherRequest
    {
        public String VoucherCode { get; set; }

        public Guid EstateId { get; set; }

        public DateTime? RedeemedDateTime { get; set; }
    }
}