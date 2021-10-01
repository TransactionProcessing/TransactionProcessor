namespace TransactionProcessor.Settlement.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record SettlementCompletedEvent : DomainEventRecord.DomainEvent
    {
        #region Properties

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        public Guid SettlementId { get; init; }

        #endregion

        public SettlementCompletedEvent(Guid aggregateId,
                                               Guid estateId) : base(aggregateId, Guid.NewGuid())
        {
            this.EstateId = estateId;
            this.SettlementId = aggregateId;
        }
    }
}