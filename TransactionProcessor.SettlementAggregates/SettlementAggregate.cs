namespace TransactionProcessor.SettlementAggregates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Models;
    using Settlement.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;

    public class SettlementAggregate : Aggregate
    {
        #region Fields

        /// <summary>
        /// The calculated fees
        /// </summary>
        private readonly List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> CalculatedFeesPendingSettlement;

        private readonly List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> SettledCalculatedFees;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SettlementAggregate" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public SettlementAggregate()
        {
            this.CalculatedFeesPendingSettlement = new List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)>();
            this.SettledCalculatedFees = new List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettlementAggregate" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private SettlementAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.CalculatedFeesPendingSettlement = new List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)>();
            this.SettledCalculatedFees = new List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)>();
        }

        #endregion

        #region Properties

        public Guid EstateId { get; private set; }

        public Boolean IsCreated { get; private set; }

        public DateTime SettlementDate { get; private set; }

        public Boolean SettlementComplete { get; private set; }

        #endregion

        #region Methods

        public void MarkFeeAsSettled(Guid merchantId, Guid transactionId, Guid feeId)
        {
            var pendingFee = this.CalculatedFeesPendingSettlement.Where(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId)
                          .SingleOrDefault();

            var settledFee = this.SettledCalculatedFees.Where(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId)
                          .SingleOrDefault();

            if (settledFee!= default((Guid, Guid, CalculatedFee)))
            {
                // Fee already settled....
                return;
            }

            if (pendingFee == default((Guid, Guid, CalculatedFee)))
            {
                // Fee not found....
                return;
            }

            MerchantFeeSettledEvent merchantFeeSettledEvent = new MerchantFeeSettledEvent(this.AggregateId,
                                                                                          this.EstateId,
                                                                                          pendingFee.merchantId,
                                                                                          pendingFee.transactionId,
                                                                                          pendingFee.calculatedFee.CalculatedValue,
                                                                                          (Int32)pendingFee.calculatedFee.FeeCalculationType,
                                                                                          pendingFee.calculatedFee.FeeId,
                                                                                          pendingFee.calculatedFee.FeeValue,
                                                                                          pendingFee.calculatedFee.FeeCalculatedDateTime);

            this.ApplyAndAppend(merchantFeeSettledEvent);

            if (this.CalculatedFeesPendingSettlement.Any() == false)
            {
                // Settlement is completed
                SettlementCompletedEvent pendingSettlementCompletedEvent = new SettlementCompletedEvent(this.AggregateId, this.EstateId);
                this.ApplyAndAppend(pendingSettlementCompletedEvent);
            }
        }

        public void ImmediatelyMarkFeeAsSettled(Guid merchantId, Guid transactionId, Guid feeId)
        {
            var pendingFee = this.CalculatedFeesPendingSettlement.Where(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId)
                          .SingleOrDefault();

            var settledFee = this.SettledCalculatedFees.Where(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId)
                          .SingleOrDefault();

            if (settledFee != default((Guid, Guid, CalculatedFee)))
            {
                // Fee already settled....
                return;
            }

            if (pendingFee == default((Guid, Guid, CalculatedFee)))
            {
                // Fee not found....
                return;
            }

            MerchantFeeSettledEvent merchantFeeSettledEvent = new MerchantFeeSettledEvent(this.AggregateId,
                                                                                          this.EstateId,
                                                                                          pendingFee.merchantId,
                                                                                          pendingFee.transactionId,
                                                                                          pendingFee.calculatedFee.CalculatedValue,
                                                                                          (Int32)pendingFee.calculatedFee.FeeCalculationType,
                                                                                          pendingFee.calculatedFee.FeeId,
                                                                                          pendingFee.calculatedFee.FeeValue,
                                                                                          pendingFee.calculatedFee.FeeCalculatedDateTime);

            this.ApplyAndAppend(merchantFeeSettledEvent);
        }

        public void AddFee(Guid merchantId,
                           Guid transactionId,
                           CalculatedFee calculatedFee)
        {
            Guard.ThrowIfInvalidGuid(merchantId, nameof(merchantId));
            Guard.ThrowIfInvalidGuid(transactionId, nameof(merchantId));
            Guard.ThrowIfNull(calculatedFee, nameof(calculatedFee));

            this.CheckHasBeenCreated();
            if (this.HasFeeAlreadyBeenAdded(transactionId, calculatedFee))
                return;

            DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.Merchant)
            {
                // This is a merchant fee
                @event = new MerchantFeeAddedPendingSettlementEvent(this.AggregateId,
                                                                      this.EstateId,
                                                                      merchantId,
                                                                      transactionId,
                                                                      calculatedFee.CalculatedValue,
                                                                      (Int32)calculatedFee.FeeCalculationType,
                                                                      calculatedFee.FeeId,
                                                                      calculatedFee.FeeValue,
                                                                      calculatedFee.FeeCalculatedDateTime);
            }
            else
            {
                throw new InvalidOperationException("Unsupported Fee Type");
            }

            this.ApplyAndAppend(@event);
        }

        public List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> GetFeesToBeSettled()
        {
            return this.CalculatedFeesPendingSettlement;
        }

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public static SettlementAggregate Create(Guid aggregateId)
        {
            return new SettlementAggregate(aggregateId);
        }

        public void Create(Guid estateId,
                           DateTime settlementDate)
        {
            this.CheckHasNotAlreadyBeenCreated();

            SettlementCreatedForDateEvent pendingSettlementCreatedForDateEvent =
                new SettlementCreatedForDateEvent(this.AggregateId, estateId, settlementDate.Date);

            this.ApplyAndAppend(pendingSettlementCreatedForDateEvent);
        }

        public Int32 GetNumberOfFeesPendingSettlement()
        {
            return this.CalculatedFeesPendingSettlement.Count;
        }

        public Int32 GetNumberOfFeesSettled()
        {
            return this.SettledCalculatedFees.Count;
        }

        public override void PlayEvent(IDomainEvent domainEvent)
        {
            this.PlayEvent((dynamic)domainEvent);
        }

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }

        private Boolean HasFeeAlreadyBeenAdded(Guid transactionId, CalculatedFee calculatedFee)
        {
            return this.CalculatedFeesPendingSettlement.Any(c => c.calculatedFee.FeeId == calculatedFee.FeeId && c.transactionId == transactionId) ||
                   this.SettledCalculatedFees.Any(c => c.calculatedFee.FeeId == calculatedFee.FeeId && c.transactionId == transactionId);
        }

        private void CheckHasBeenCreated()
        {
            if (this.IsCreated == false)
            {
                throw new InvalidOperationException($"Pending Settlement not created for this date {this.SettlementDate}");
            }
        }

        private void CheckHasNotAlreadyBeenCreated()
        {
            if (this.IsCreated)
            {
                throw new InvalidOperationException($"Pending Settlement already created for this date {this.SettlementDate}");
            }
        }

        private void PlayEvent(MerchantFeeSettledEvent domainEvent)
        {
            // Add to the settled fees list
            this.SettledCalculatedFees.Add(new(domainEvent.TransactionId,
                                                   domainEvent.MerchantId,
                                                   new CalculatedFee
                                                   {
                                                       CalculatedValue = domainEvent.CalculatedValue,
                                                       FeeId = domainEvent.FeeId,
                                                       FeeType = FeeType.Merchant,
                                                       FeeValue = domainEvent.FeeValue,
                                                       FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType
                                                   }));

            // Remove from the pending list
            var feeToRemove = this.CalculatedFeesPendingSettlement
                                  .Single(f => f.transactionId == domainEvent.TransactionId &&
                                               f.merchantId == domainEvent.MerchantId && f.calculatedFee.FeeId == domainEvent.FeeId);

            this.CalculatedFeesPendingSettlement.Remove(feeToRemove);
        }

        private void PlayEvent(MerchantFeeAddedPendingSettlementEvent domainEvent)
        {
            this.CalculatedFeesPendingSettlement.Add(new(domainEvent.TransactionId,
                                                         domainEvent.MerchantId,
                                                         new CalculatedFee
                                                         {
                                                             CalculatedValue = domainEvent.CalculatedValue,
                                                             FeeId = domainEvent.FeeId,
                                                             FeeType = FeeType.Merchant,
                                                             FeeValue = domainEvent.FeeValue,
                                                             FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType
                                                         }));
        }

        private void PlayEvent(SettlementCreatedForDateEvent domainEvent)
        {
            this.EstateId = domainEvent.EstateId;
            this.SettlementDate = domainEvent.SettlementDate;
            this.IsCreated = true;
        }

        private void PlayEvent(SettlementCompletedEvent domainEvent)
        {
            this.SettlementComplete = true;
        }

        #endregion
    }
}