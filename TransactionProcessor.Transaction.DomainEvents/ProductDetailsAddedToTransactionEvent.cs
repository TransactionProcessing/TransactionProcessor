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
    public class ProductDetailsAddedToTransactionEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDetailsAddedToTransactionEvent"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ProductDetailsAddedToTransactionEvent()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductDetailsAddedToTransactionEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="productId">The product identifier.</param>
        private ProductDetailsAddedToTransactionEvent(Guid aggregateId,
                                                      Guid eventId,
                                                      Guid estateId,
                                                      Guid contractId,
                                                      Guid productId) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.ContractId = contractId;
            this.ProductId = productId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the contract identifier.
        /// </summary>
        /// <value>
        /// The contract identifier.
        /// </value>
        [JsonProperty]
        public Guid ContractId { get; private set; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty]
        public Guid EstateId { get; private set; }

        /// <summary>
        /// Gets the product identifier.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        [JsonProperty]
        public Guid ProductId { get; private set; }

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
        /// <param name="contractId">The contract identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <returns></returns>
        public static ProductDetailsAddedToTransactionEvent Create(Guid aggregateId,
                                                                   Guid estateId,
                                                                   Guid contractId,
                                                                   Guid productId)
        {
            return new ProductDetailsAddedToTransactionEvent(aggregateId, Guid.NewGuid(), estateId, contractId, productId);
        }

        #endregion
    }
}