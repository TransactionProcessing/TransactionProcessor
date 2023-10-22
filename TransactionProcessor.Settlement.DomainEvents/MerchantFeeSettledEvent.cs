using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionProcessor.Settlement.DomainEvents
{
    using Shared.DomainDrivenDesign.EventSourcing;

    public record MerchantFeeSettledEvent : DomainEvent
    {
        #region Properties

        public Decimal CalculatedValue { get; init; }

        public DateTime FeeCalculatedDateTime { get; init; }

        public DateTime SettledDateTime{ get; init; }

        public Guid EstateId { get; init; }

        public Int32 FeeCalculationType { get; init; }

        public Guid FeeId { get; init; }

        public Decimal FeeValue { get; init; }

        public Guid MerchantId { get; init; }

        public Guid TransactionId { get; init; }

        public Guid SettlementId { get; init; }

        #endregion

        public MerchantFeeSettledEvent(Guid aggregateId,
                                       Guid estateId,
                                       Guid merchantId,
                                       Guid transactionId,
                                       Decimal calculatedValue,
                                       Int32 feeCalculationType,
                                       Guid feeId,
                                       Decimal feeValue,
                                       DateTime feeCalculatedDateTime,
                                       DateTime settledDateTime) : base(aggregateId, Guid.NewGuid())
        {
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.TransactionId = transactionId;
            this.CalculatedValue = calculatedValue;
            this.FeeCalculationType = feeCalculationType;
            this.FeeId = feeId;
            this.FeeValue = feeValue;
            this.FeeCalculatedDateTime = feeCalculatedDateTime;
            this.SettledDateTime = settledDateTime;
            this.SettlementId = aggregateId;
        }
    }
}