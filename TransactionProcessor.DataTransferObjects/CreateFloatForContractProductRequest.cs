using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.DataTransferObjects{
    public class CreateFloatForContractProductRequest{
        
        public Guid ContractId{ get; set; }

        public Guid ProductId{ get; set; }

        public DateTime CreateDateTime{ get; set; }
    }

    public class CreateFloatForContractProductResponse
    {
        public Guid FloatId { get; set; }
    }

    public class RecordFloatCreditPurchaseRequest{
        public Guid FloatId { get; set; }
        public DateTime PurchaseDateTime { get; set; }
        public Decimal CreditAmount { get; set; }
        public Decimal CostPrice { get; set; }
    }
}
