namespace TransactionProcessor.Transaction.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.EventSourcing.DomainEvent" />
    public record SettledMerchantFeeAddedToTransactionEvent : DomainEvent
    {
        #region Constructors

        public SettledMerchantFeeAddedToTransactionEvent(Guid aggregateId,
                                                  Guid estateId,
                                                  Guid merchantId,
                                                  Decimal calculatedValue,
                                                  Int32 feeCalculationType,
                                                  Guid feeId,
                                                  Decimal feeValue,
                                                  DateTime feeCalculatedDateTime,
                                                  DateTime settledDateTime, 
                                                  Guid settlementId) : base(aggregateId, Guid.NewGuid())
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.CalculatedValue = calculatedValue;
            this.FeeCalculationType = feeCalculationType;
            this.FeeId = feeId;
            this.FeeValue = feeValue;
            this.FeeCalculatedDateTime = feeCalculatedDateTime;
            this.SettledDateTime = settledDateTime;
            this.SettlementId = settlementId;
        }

        #endregion

        #region Properties

        public Decimal CalculatedValue { get; init; }

        public DateTime FeeCalculatedDateTime { get; init; }
       
        public DateTime SettledDateTime { get; init; }

        public Guid SettlementId{ get; init; }

        public Guid EstateId { get; init; }

        public Int32 FeeCalculationType { get; init; }

        public Guid FeeId { get; init; }

        public Decimal FeeValue { get; init; }

        public Guid MerchantId { get; init; }

        public Guid TransactionId { get; init; }

        #endregion
    }


    public record MerchantFeePendingSettlementAddedToTransactionEvent : DomainEvent
    {
        #region Constructors

        public MerchantFeePendingSettlementAddedToTransactionEvent(Guid aggregateId,
                                                                   Guid estateId,
                                                                   Guid merchantId,
                                                                   Decimal calculatedValue,
                                                                   Int32 feeCalculationType,
                                                                   Guid feeId,
                                                                   Decimal feeValue,
                                                                   DateTime feeCalculatedDateTime,
                                                                   DateTime settlementDueDate) : base(aggregateId, Guid.NewGuid())
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.CalculatedValue = calculatedValue;
            this.FeeCalculationType = feeCalculationType;
            this.FeeId = feeId;
            this.FeeValue = feeValue;
            this.FeeCalculatedDateTime = feeCalculatedDateTime;
            this.SettlementDueDate = settlementDueDate;
        }

        #endregion

        #region Properties

        public Decimal CalculatedValue { get; init; }

        public DateTime FeeCalculatedDateTime { get; init; }

        public DateTime SettlementDueDate { get; init; }

        public Guid EstateId { get; init; }

        public Int32 FeeCalculationType { get; init; }

        public Guid FeeId { get; init; }

        public Decimal FeeValue { get; init; }

        public Guid MerchantId { get; init; }

        public Guid TransactionId { get; init; }

        #endregion
    }
}