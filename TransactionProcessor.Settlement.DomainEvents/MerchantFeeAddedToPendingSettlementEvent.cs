namespace TransactionProcessor.Settlement.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record MerchantFeeAddedToPendingSettlementEvent : DomainEventRecord.DomainEvent
    {
        #region Properties

        /// <summary>
        /// Gets the calculated value.
        /// </summary>
        /// <value>
        /// The calculated value.
        /// </value>
        public Decimal CalculatedValue { get; init; }

        /// <summary>
        /// Gets or sets the fee calculated date time.
        /// </summary>
        /// <value>
        /// The fee calculated date time.
        /// </value>
        public DateTime FeeCalculatedDateTime { get; init; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets the type of the fee calculation.
        /// </summary>
        /// <value>
        /// The type of the fee calculation.
        /// </value>
        public Int32 FeeCalculationType { get; init; }

        /// <summary>
        /// Gets the fee identifier.
        /// </summary>
        /// <value>
        /// The fee identifier.
        /// </value>
        public Guid FeeId { get; init; }

        /// <summary>
        /// Gets the fee value.
        /// </summary>
        /// <value>
        /// The fee value.
        /// </value>
        public Decimal FeeValue { get; init; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; init; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public Guid TransactionId { get; init; }

        #endregion

        public MerchantFeeAddedToPendingSettlementEvent(Guid aggregateId,
                                                        Guid estateId,
                                                        Guid merchantId,
                                                        Guid transactionId,
                                                        Decimal calculatedValue,
                                                        Int32 feeCalculationType,
                                                        Guid feeId,
                                                        Decimal feeValue,
                                                        DateTime feeCalculatedDateTime) : base(aggregateId, Guid.NewGuid())
        {
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.TransactionId = transactionId;
            this.CalculatedValue = calculatedValue;
            this.FeeCalculationType = feeCalculationType;
            this.FeeId = feeId;
            this.FeeValue = feeValue;
            this.FeeCalculatedDateTime = feeCalculatedDateTime;
        }
    }
}