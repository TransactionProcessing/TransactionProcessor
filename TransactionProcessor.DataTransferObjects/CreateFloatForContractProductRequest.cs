using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.DataTransferObjects{
    using Newtonsoft.Json;

    public class CreateFloatForContractProductRequest{

        [JsonProperty("contract_id")]
        public Guid ContractId{ get; set; }

        [JsonProperty("product_id")]
        public Guid ProductId{ get; set; }

        [JsonProperty("create_date_time")]
        public DateTime CreateDateTime{ get; set; }
    }

    public class CreateFloatForContractProductResponse
    {
        [JsonProperty("float_id")]
        public Guid FloatId { get; set; }
    }

    public class RecordFloatCreditPurchaseRequest{
        [JsonProperty("float_id")]
        public Guid FloatId { get; set; }

        [JsonProperty("purchase_date_time")]
        public DateTime PurchaseDateTime { get; set; }

        [JsonProperty("credit_amount")]
        public Decimal CreditAmount { get; set; }

        [JsonProperty("cost_price")]
        public Decimal CostPrice { get; set; }
    }
}
