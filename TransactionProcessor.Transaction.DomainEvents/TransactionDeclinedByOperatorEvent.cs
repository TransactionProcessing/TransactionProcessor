namespace TransactionProcessor.Transaction.DomainEvents
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record TransactionDeclinedByOperatorEvent : DomainEventRecord.DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAuthorisedByOperatorEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="operatorIdentifier">The operator identifier.</param>
        /// <param name="operatorResponseCode">The operator response code.</param>
        /// <param name="operatorResponseMessage">The operator response message.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public TransactionDeclinedByOperatorEvent(Guid aggregateId,

                                               Guid estateId,
                                               Guid merchantId,
                                               String operatorIdentifier,
                                               String operatorResponseCode,
                                               String operatorResponseMessage,
                                               String responseCode,
                                               String responseMessage) : base(aggregateId, Guid.NewGuid())
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.OperatorIdentifier = operatorIdentifier;
            this.OperatorResponseCode = operatorResponseCode;
            this.OperatorResponseMessage = operatorResponseMessage;
            this.ResponseCode = responseCode;
            this.ResponseMessage = responseMessage;
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; init; }

        /// <summary>
        /// Gets the operator identifier.
        /// </summary>
        /// <value>
        /// The operator identifier.
        /// </value>
        public String OperatorIdentifier { get; init; }

        /// <summary>
        /// Gets the operator response code.
        /// </summary>
        /// <value>
        /// The operator response code.
        /// </value>
        public String OperatorResponseCode { get; init; }

        /// <summary>
        /// Gets the operator response message.
        /// </summary>
        /// <value>
        /// The operator response message.
        /// </value>
        public String OperatorResponseMessage { get; init; }

        /// <summary>
        /// Gets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public String ResponseCode { get; init; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; init; }

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