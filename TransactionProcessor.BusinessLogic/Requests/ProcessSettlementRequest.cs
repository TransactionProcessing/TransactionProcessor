namespace TransactionProcessor.BusinessLogic.Requests{
    using System;
    using MediatR;
    using Models;

    public class ProcessSettlementRequest : IRequest<ProcessSettlementResponse>{
        #region Constructors

        private ProcessSettlementRequest(DateTime settlementDate, Guid merchantId, Guid estateId){
            this.MerchantId = merchantId;
            this.EstateId = estateId;
            this.SettlementDate = settlementDate;
        }

        #endregion

        #region Properties

        public Guid EstateId{ get; }

        public Guid MerchantId{ get; }

        public DateTime SettlementDate{ get; }

        #endregion

        #region Methods

        public static ProcessSettlementRequest Create(DateTime settlementDate,
                                                      Guid merchantId,
                                                      Guid estateId){
            return new ProcessSettlementRequest(settlementDate, merchantId, estateId);
        }

        #endregion
    }
}