using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.Transaction.DomainEvents
{
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
    using Shared.DomainDrivenDesign.EventSourcing;

    public class CustomerEmailReceiptRequestedEvent : DomainEvent
    {
        [JsonProperty]
        public Guid EstateId { get; private set; }

        [JsonProperty]
        public Guid MerchantId { get; private set; }

        [JsonProperty]
        public String CustomerEmailAddress { get; private set; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty]
        public Guid TransactionId { get; private set; }

        [ExcludeFromCodeCoverage]
        public CustomerEmailReceiptRequestedEvent()
        {

        }

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

        public static CustomerEmailReceiptRequestedEvent Create(Guid aggregateId,
                                                                Guid estateId,
                                                                Guid merchantId,
                                                                String customerEmailAddress)
        {
            return new CustomerEmailReceiptRequestedEvent(aggregateId,Guid.NewGuid(), estateId,merchantId, customerEmailAddress);
        }
    }
}
