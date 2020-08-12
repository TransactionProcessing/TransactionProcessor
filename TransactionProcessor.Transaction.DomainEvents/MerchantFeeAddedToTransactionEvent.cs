namespace TransactionProcessor.Transaction.DomainEvents
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.EventSourcing.DomainEvent" />
    [JsonObject]
    public class MerchantFeeAddedToTransactionEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantFeeAddedToTransactionEvent"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MerchantFeeAddedToTransactionEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MerchantFeeAddedToTransactionEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="calculatedValue">The calculated value.</param>
        /// <param name="feeCalculationType">Type of the fee calculation.</param>
        /// <param name="feeId">The fee identifier.</param>
        /// <param name="feeValue">The fee value.</param>
        private MerchantFeeAddedToTransactionEvent(Guid aggregateId,
                                                   Guid eventId,
                                                   Guid estateId,
                                                   Guid merchantId,
                                                   Decimal calculatedValue,
                                                   Int32 feeCalculationType,
                                                   Guid feeId,
                                                   Decimal feeValue) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.CalculatedValue = calculatedValue;
            this.FeeCalculationType = feeCalculationType;
            this.FeeId = feeId;
            this.FeeValue = feeValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the calculated value.
        /// </summary>
        /// <value>
        /// The calculated value.
        /// </value>
        [JsonProperty]
        public Decimal CalculatedValue { get; private set; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty]
        public Guid EstateId { get; private set; }

        /// <summary>
        /// Gets the type of the fee calculation.
        /// </summary>
        /// <value>
        /// The type of the fee calculation.
        /// </value>
        [JsonProperty]
        public Int32 FeeCalculationType { get; private set; }

        /// <summary>
        /// Gets the fee identifier.
        /// </summary>
        /// <value>
        /// The fee identifier.
        /// </value>
        [JsonProperty]
        public Guid FeeId { get; private set; }

        /// <summary>
        /// Gets the fee value.
        /// </summary>
        /// <value>
        /// The fee value.
        /// </value>
        [JsonProperty]
        public Decimal FeeValue { get; private set; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        [JsonProperty]
        public Guid MerchantId { get; private set; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty]
        public Guid TransactionId { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="calculatedValue">The calculated value.</param>
        /// <param name="feeCalculationType">Type of the fee calculation.</param>
        /// <param name="feeId">The fee identifier.</param>
        /// <param name="feeValue">The fee value.</param>
        /// <returns></returns>
        public static MerchantFeeAddedToTransactionEvent Create(Guid aggregateId,
                                                                Guid estateId,
                                                                Guid merchantId,
                                                                Decimal calculatedValue,
                                                                Int32 feeCalculationType,
                                                                Guid feeId,
                                                                Decimal feeValue)
        {
            return new MerchantFeeAddedToTransactionEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, calculatedValue, feeCalculationType, feeId, feeValue);
        }

        #endregion
    }
}