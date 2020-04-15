namespace TransactionProcessor.Transaction.DomainEvents
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    [JsonObject]
    public class TransactionAuthorisedByOperatorEvent : DomainEvent
    {
        #region Constructors

        [ExcludeFromCodeCoverage]
        public TransactionAuthorisedByOperatorEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAuthorisedByOperatorEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="authorisationCode">The authorisation code.</param>
        /// <param name="operatorResponseCode">The operator response code.</param>
        /// <param name="operatorResponseMessage">The operator response message.</param>
        /// <param name="operatorTransactionId">The operator transaction identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public TransactionAuthorisedByOperatorEvent(Guid aggregateId,
                                                    Guid eventId,
                                                    Guid estateId,
                                                    Guid merchantId,
                                                    String operatorIdentifier,
                                                    String authorisationCode,
                                                    String operatorResponseCode,
                                                    String operatorResponseMessage,
                                                    String operatorTransactionId,
                                                    String responseCode,
                                                    String responseMessage) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.OperatorIdentifier = operatorIdentifier;
            this.AuthorisationCode = authorisationCode;
            this.OperatorResponseCode = operatorResponseCode;
            this.OperatorResponseMessage = operatorResponseMessage;
            this.OperatorTransactionId = operatorTransactionId;
            this.ResponseCode = responseCode;
            this.ResponseMessage = responseMessage;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the authorisation code.
        /// </summary>
        /// <value>
        /// The authorisation code.
        /// </value>
        [JsonProperty]
        public String AuthorisationCode { get; private set; }

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
        /// Gets the operator response code.
        /// </summary>
        /// <value>
        /// The operator response code.
        /// </value>
        [JsonProperty]
        public String OperatorResponseCode { get; private set; }

        /// <summary>
        /// Gets the operator response message.
        /// </summary>
        /// <value>
        /// The operator response message.
        /// </value>
        [JsonProperty]
        public String OperatorResponseMessage { get; private set; }

        /// <summary>
        /// Gets or sets the operator transaction identifier.
        /// </summary>
        /// <value>
        /// The operator transaction identifier.
        /// </value>
        [JsonProperty]
        public String OperatorTransactionId { get; set; }

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
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="authorisationCode">The authorisation code.</param>
        /// <param name="operatorResponseCode">The operator response code.</param>
        /// <param name="operatorResponseMessage">The operator response message.</param>
        /// <param name="operatorTransactionId">The operator transaction identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <returns></returns>
        public static TransactionAuthorisedByOperatorEvent Create(Guid aggregateId,
                                                                  Guid estateId,
                                                                  Guid merchantId,
                                                                  String operatorIdentifier,
                                                                  String authorisationCode,
                                                                  String operatorResponseCode,
                                                                  String operatorResponseMessage,
                                                                  String operatorTransactionId,
                                                                  String responseCode,
                                                                  String responseMessage)
        {
            return new TransactionAuthorisedByOperatorEvent(aggregateId,
                                                            Guid.NewGuid(),
                                                            estateId,
                                                            merchantId,
                                                            operatorIdentifier,
                                                            authorisationCode,
                                                            operatorResponseCode,
                                                            operatorResponseMessage,
                                                            operatorTransactionId,
                                                            responseCode,
                                                            responseMessage);
        }

        #endregion
    }
}