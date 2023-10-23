using Shared.DomainDrivenDesign.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.Transaction.DomainEvents
{
    public record TransactionCostInformationRecordedEvent : DomainEvent
    {
        #region Constructors

        public TransactionCostInformationRecordedEvent(Guid aggregateId,
                                                       Guid estateId,
                                                       Guid merchantId,
                                                       Decimal? unitCostValue,
                                                       Decimal? totalCostValue) : base(aggregateId, Guid.NewGuid())
        {
            this.TransactionId = aggregateId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.UnitCostValue = unitCostValue;
            this.TotalCostValue = totalCostValue;
        }

        #endregion

        #region Properties

        public Guid EstateId { get; init; }

        public Guid MerchantId { get; init; }

        public Guid TransactionId { get; init; }

        public Decimal? UnitCostValue { get; init; }

        public Decimal? TotalCostValue { get; init; }

        #endregion
    }
}
