namespace TransactionProcessor.Settlement.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record PendingSettlementCreatedForDateEvent : DomainEventRecord.DomainEvent
    {
        #region Properties
        
        public DateTime SettlementDate { get; init; }

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }
        
        #endregion

        public PendingSettlementCreatedForDateEvent(Guid aggregateId,
                                                    Guid estateId,
                                                    DateTime settlementDate) : base(aggregateId, Guid.NewGuid())
        {
            this.EstateId = estateId;
            this.SettlementDate = settlementDate;
        }
    }
}