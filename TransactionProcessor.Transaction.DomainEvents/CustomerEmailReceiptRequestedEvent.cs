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
    public class CustomerEmailReceiptRequestedEvent : DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerEmailReceiptRequestedEvent"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public CustomerEmailReceiptRequestedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerEmailReceiptRequestedEvent"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="customerEmailAddress">The customer email address.</param>
        private CustomerEmailReceiptRequestedEvent(Guid aggregateId,
                                                   Guid eventId,
                                                   Guid estateId,
                                                   Guid merchantId,
                                                   String customerEmailAddress) : base(aggregateId, eventId)
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.CustomerEmailAddress = customerEmailAddress;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the customer email address.
        /// </summary>
        /// <value>
        /// The customer email address.
        /// </value>
        [JsonProperty]
        public String CustomerEmailAddress { get; private set; }

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
        /// <param name="customerEmailAddress">The customer email address.</param>
        /// <returns></returns>
        public static CustomerEmailReceiptRequestedEvent Create(Guid aggregateId,
                                                                Guid estateId,
                                                                Guid merchantId,
                                                                String customerEmailAddress)
        {
            return new CustomerEmailReceiptRequestedEvent(aggregateId, Guid.NewGuid(), estateId, merchantId, customerEmailAddress);
        }

        #endregion
    }
}