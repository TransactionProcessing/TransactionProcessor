namespace TransactionProcessor.Transaction.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.EventSourcing.DomainEvent" />
    public record MerchantFeeAddedToTransactionEvent : DomainEventRecord.DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantFeeAddedToTransactionEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="calculatedValue">The calculated value.</param>
        /// <param name="feeCalculationType">Type of the fee calculation.</param>
        /// <param name="feeId">The fee identifier.</param>
        /// <param name="feeValue">The fee value.</param>
        /// <param name="feeCalculatedDateTime"></param>
        public MerchantFeeAddedToTransactionEvent(Guid aggregateId,
                                                  Guid estateId,
                                                  Guid merchantId,
                                                  Decimal calculatedValue,
                                                  Int32 feeCalculationType,
                                                  Guid feeId,
                                                  Decimal feeValue,
                                                  DateTime feeCalculatedDateTime,
                                                  DateTime settlementDueDate,
                                                  DateTime settledDateTime) : base(aggregateId, Guid.NewGuid())
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
            this.SettledDateTime = settledDateTime;
        }

        #endregion

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

        public DateTime SettlementDueDate { get; init; }
        
        public DateTime SettledDateTime { get; init; }

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
    }
}