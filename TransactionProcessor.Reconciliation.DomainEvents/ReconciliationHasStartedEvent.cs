using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.Reconciliation.DomainEvents
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.EventSourcing.DomainEvent" />
    [JsonObject]
    public class ReconciliationHasStartedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconciliationHasStartedEvent" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ReconciliationHasStartedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconciliationHasStartedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="transactionDateTime">The transaction date time.</param>
        private ReconciliationHasStartedEvent(Guid aggregateId,
                                              Guid eventId,
                                              Guid estateId,
                                              Guid merchantId,
                                              DateTime transactionDateTime) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.TransactionDateTime = transactionDateTime;
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
        /// Gets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        [JsonProperty]
        public DateTime TransactionDateTime { get; private set; }

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
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <returns></returns>
        public static ReconciliationHasStartedEvent Create(Guid aggregateId,
                                                           Guid estateId,
                                                           Guid merchantId,
                                                           DateTime transactionDateTime)
        {
            return new ReconciliationHasStartedEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, transactionDateTime);
        }

        #endregion
    }
}
