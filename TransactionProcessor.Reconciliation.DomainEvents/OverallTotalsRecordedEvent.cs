namespace TransactionProcessor.Reconciliation.DomainEvents
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
    public class OverallTotalsRecordedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OverallTotalsRecordedEvent" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public OverallTotalsRecordedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OverallTotalsRecordedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="transactionCount">The transaction count.</param>
        /// <param name="transactionValue">The transaction value.</param>
        private OverallTotalsRecordedEvent(Guid aggregateId,
                                           Guid eventId,
                                           Guid estateId,
                                           Guid merchantId,
                                           Int32 transactionCount,
                                           Decimal transactionValue) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.TransactionCount = transactionCount;
            this.TransactionValue = transactionValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty]
        public Guid EstateId { get; private set; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        [JsonProperty]
        public Guid MerchantId { get; private set; }

        /// <summary>
        /// Gets the transaction count.
        /// </summary>
        /// <value>
        /// The transaction count.
        /// </value>
        [JsonProperty]
        public Int32 TransactionCount { get; private set; }

        /// <summary>
        /// Gets the transaction value.
        /// </summary>
        /// <value>
        /// The transaction value.
        /// </value>
        [JsonProperty]
        public Decimal TransactionValue { get; private set; }
        
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
        /// <param name="transactionCount">The transaction count.</param>
        /// <param name="transactionValue">The transaction value.</param>
        /// <returns></returns>
        public static OverallTotalsRecordedEvent Create(Guid aggregateId,
                                                        Guid estateId,
                                                        Guid merchantId,
                                                        Int32 transactionCount,
                                                        Decimal transactionValue)
        {
            return new OverallTotalsRecordedEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, transactionCount, transactionValue);
        }

        #endregion
    }
}