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

    public class PendingSettlementAggregate : Aggregate
    {
        #region Fields

        /// <summary>
        /// The calculated fees
        /// </summary>
        private readonly List<(Guid transactionId, CalculatedFee calculatedFee)> CalculatedFeesPendingSettlement;

        private readonly List<(Guid transactionId, CalculatedFee calculatedFee)> SettledCalculatedFees;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PendingSettlementAggregate" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public PendingSettlementAggregate()
        {
            this.CalculatedFeesPendingSettlement = new List<(Guid transactionId, CalculatedFee calculatedFee)>();
            this.SettledCalculatedFees = new List<(Guid transactionId, CalculatedFee calculatedFee)>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAggregate" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private PendingSettlementAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.CalculatedFeesPendingSettlement = new List<(Guid transactionId, CalculatedFee calculatedFee)>();
            this.SettledCalculatedFees = new List<(Guid transactionId, CalculatedFee calculatedFee)>();
        }

        #endregion

        #region Properties

        public Guid EstateId { get; private set; }

        public Boolean IsCreated { get; private set; }

        public DateTime SettlmentDate { get; private set; }

        #endregion

        #region Methods

        public void AddFee(Guid merchantId,
                           Guid transactionId,
                           CalculatedFee calculatedFee)
        {
            if (calculatedFee == null)
            {
                throw new ArgumentNullException(nameof(calculatedFee));
            }

            this.CheckHasBeenCreated();
            this.CheckFeeHasNotAlreadyBeenAdded(calculatedFee);

            DomainEventRecord.DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.Merchant)
            {
                // This is a merchant fee
                @event = new MerchantFeeAddedToPendingSettlementEvent(this.AggregateId,
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

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public static PendingSettlementAggregate Create(Guid aggregateId)
        {
            return new PendingSettlementAggregate(aggregateId);
        }

        public void Create(Guid estateId,
                           DateTime settlementDate)
        {
            this.CheckHasNotAlreadyBeenCreated();

            PendingSettlementCreatedForDateEvent pendingSettlementCreatedForDateEvent =
                new PendingSettlementCreatedForDateEvent(this.AggregateId, estateId, settlementDate.Date);

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

        protected override Object GetMetadata()
        {
            return null;
        }

        private void CheckFeeHasNotAlreadyBeenAdded(CalculatedFee calculatedFee)
        {
            if (this.CalculatedFeesPendingSettlement.Any(c => c.calculatedFee.FeeId == calculatedFee.FeeId))
            {
                throw new InvalidOperationException($"Fee with Id [{calculatedFee.FeeId}] has already been added to this days pending settlement");
            }
        }

        private void CheckHasBeenCreated()
        {
            if (this.IsCreated == false)
            {
                throw new InvalidOperationException($"Pending Settlement not created for this date {this.SettlmentDate}");
            }
        }

        private void CheckHasNotAlreadyBeenCreated()
        {
            if (this.IsCreated)
            {
                throw new InvalidOperationException($"Pending Settlement already created for this date {this.SettlmentDate}");
            }
        }

        private void PlayEvent(MerchantFeeAddedToPendingSettlementEvent domainEvent)
        {
            this.CalculatedFeesPendingSettlement.Add(new(domainEvent.TransactionId,
                                                         new CalculatedFee
                                                         {
                                                             CalculatedValue = domainEvent.CalculatedValue,
                                                             FeeId = domainEvent.FeeId,
                                                             FeeType = FeeType.Merchant,
                                                             FeeValue = domainEvent.FeeValue,
                                                             FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType
                                                         }));
        }

        private void PlayEvent(PendingSettlementCreatedForDateEvent domainEvent)
        {
            this.EstateId = domainEvent.EstateId;
            this.SettlmentDate = domainEvent.SettlementDate;
            this.IsCreated = true;
        }

        #endregion
    }
}