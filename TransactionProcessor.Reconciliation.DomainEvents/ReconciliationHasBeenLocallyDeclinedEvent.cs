namespace TransactionProcessor.Transaction.DomainEvents
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    [JsonObject]
    public class ReconciliationHasBeenLocallyDeclinedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconciliationHasBeenLocallyDeclinedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        private ReconciliationHasBeenLocallyDeclinedEvent(Guid aggregateId,
                                                      Guid eventId,
                                                      Guid estateId,
                                                      Guid merchantId,
                                                      String responseCode,
                                                      String responseMessage) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.ResponseCode = responseCode;
            this.ResponseMessage = responseMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconciliationHasBeenLocallyDeclinedEvent" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ReconciliationHasBeenLocallyDeclinedEvent()
        {
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
        /// Gets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        [JsonProperty]
        public String ResponseCode { get; private set; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        [JsonProperty]
        public String ResponseMessage { get; private set; }

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
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <returns></returns>
        public static ReconciliationHasBeenLocallyDeclinedEvent Create(Guid aggregateId,
                                                                      Guid estateId,
                                                                      Guid merchantId,
                                                                      String responseCode,
                                                                      String responseMessage)
        {
            return new ReconciliationHasBeenLocallyDeclinedEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, responseCode, responseMessage);
        }

        #endregion
    }
}