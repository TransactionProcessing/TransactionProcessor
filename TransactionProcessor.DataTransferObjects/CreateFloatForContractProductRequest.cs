using System;

namespace TransactionProcessor.DataTransferObjects{

    public class CreateFloatRequest{

        public Guid FloatId { get; set; }

        public DateTime CreateDateTime{ get; set; }
    }
    
    public class RecordFloatCreditPurchaseRequest{
        public Guid FloatId { get; set; }

        public DateTime PurchaseDateTime { get; set; }

        public Decimal CreditAmount { get; set; }
        public Decimal CostPrice { get; set; }
    }
}
