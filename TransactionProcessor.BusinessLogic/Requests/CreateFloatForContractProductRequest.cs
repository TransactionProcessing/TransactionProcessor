using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.BusinessLogic.Requests
{
    using MediatR;
    using TransactionProcessor.Models;

    public class CreateFloatForContractProductRequest : IRequest<CreateFloatForContractProductResponse>
    {
        public Guid EstateId { get; }

        public Guid ContractId{ get; }

        public Guid ProductId { get; }

        public DateTime CreateDateTime { get; }

        private CreateFloatForContractProductRequest(Guid estateId,
                                                     Guid contractId,
                                                     Guid productId,
                                                     DateTime createDateTime){
            this.EstateId = estateId;
            this.ContractId = contractId;
            this.ProductId = productId;
            this.CreateDateTime = createDateTime;
        }

        public static CreateFloatForContractProductRequest Create(Guid estateId,
                                                                  Guid contractId,
                                                                  Guid productId,
                                                                  DateTime createDateTime){
            return new CreateFloatForContractProductRequest(estateId, contractId, productId, createDateTime);
        }
    }

    public class RecordCreditPurchaseForFloatRequest : IRequest{
        public Guid EstateId { get; }
        public Guid FloatId { get; }
        public Decimal CreditAmount { get; }
        public Decimal CostPrice { get; }
        public DateTime PurchaseDateTime { get; }

        private RecordCreditPurchaseForFloatRequest(Guid estateId, Guid floatId, Decimal creditAmount, Decimal costPrice, DateTime purchaseDateTime){
            this.EstateId = estateId;
            this.FloatId = floatId;
            this.CreditAmount = creditAmount;
            this.CostPrice = costPrice;
            this.PurchaseDateTime = purchaseDateTime;
        }

        public static RecordCreditPurchaseForFloatRequest Create(Guid estateId, Guid floatId, Decimal creditAmount, Decimal costPrice, DateTime purchaseDateTime){
            return new RecordCreditPurchaseForFloatRequest(estateId, floatId, creditAmount, costPrice, purchaseDateTime);
        }
    }
}
