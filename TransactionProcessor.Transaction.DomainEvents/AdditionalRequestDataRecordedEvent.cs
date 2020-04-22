namespace TransactionProcessor.Transaction.DomainEvents
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.EventSourcing.DomainEvent" />
    public class AdditionalRequestDataRecordedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalRequestDataRecordedEvent"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public AdditionalRequestDataRecordedEvent()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalRequestDataRecordedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="additionalTransactionRequestMetadata">The additional transaction request metadata.</param>
        private AdditionalRequestDataRecordedEvent(Guid aggregateId,
                                                   Guid eventId,
                                                   Guid estateId,
                                                   Guid merchantId,
                                                   String operatorIdentifier,
                                                   Dictionary<String, String> additionalTransactionRequestMetadata) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.OperatorIdentifier = operatorIdentifier;
            this.AdditionalTransactionRequestMetadata = additionalTransactionRequestMetadata;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the additional transaction request metadata.
        /// </summary>
        /// <value>
        /// The additional transaction request metadata.
        /// </value>
        [JsonProperty]
        public Dictionary<String, String> AdditionalTransactionRequestMetadata { get; private set; }

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
        /// Gets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        [JsonProperty]
        public String OperatorIdentifier { get; private set; }

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
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="additionalTransactionRequestMetadata">The additional transaction request metadata.</param>
        /// <returns></returns>
        public static AdditionalRequestDataRecordedEvent Create(Guid aggregateId,
                                                                Guid estateId,
                                                                Guid merchantId,
                                                                String operatorIdentifier,
                                                                Dictionary<String, String> additionalTransactionRequestMetadata)
        {
            return new AdditionalRequestDataRecordedEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, operatorIdentifier, additionalTransactionRequestMetadata);
        }

        #endregion
    }
}