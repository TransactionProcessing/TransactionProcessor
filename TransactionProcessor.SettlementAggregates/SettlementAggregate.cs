﻿namespace TransactionProcessor.SettlementAggregates
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

    public static class SettlementAggregateExtensions{
        public static void MarkFeeAsSettled(this SettlementAggregate aggregate, Guid merchantId, Guid transactionId, Guid feeId)
        {
            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) pendingFee = aggregate.CalculatedFeesPendingSettlement.Where(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId)
                                                                                                     .SingleOrDefault();

            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) settledFee = aggregate.SettledCalculatedFees.Where(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId)
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

            MerchantFeeSettledEvent merchantFeeSettledEvent = new MerchantFeeSettledEvent(aggregate.AggregateId,
                                                                                          aggregate.EstateId,
                                                                                          pendingFee.merchantId,
                                                                                          pendingFee.transactionId,
                                                                                          pendingFee.calculatedFee.CalculatedValue,
                                                                                          (Int32)pendingFee.calculatedFee.FeeCalculationType,
                                                                                          pendingFee.calculatedFee.FeeId,
                                                                                          pendingFee.calculatedFee.FeeValue,
                                                                                          pendingFee.calculatedFee.FeeCalculatedDateTime);

            aggregate.ApplyAndAppend(merchantFeeSettledEvent);

            if (aggregate.CalculatedFeesPendingSettlement.Any() == false)
            {
                // Settlement is completed
                SettlementCompletedEvent pendingSettlementCompletedEvent = new SettlementCompletedEvent(aggregate.AggregateId, aggregate.EstateId);
                aggregate.ApplyAndAppend(pendingSettlementCompletedEvent);
            }
        }

        public static void ImmediatelyMarkFeeAsSettled(this SettlementAggregate aggregate, Guid merchantId, Guid transactionId, Guid feeId)
        {
            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) pendingFee = aggregate.CalculatedFeesPendingSettlement.Where(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId)
                                                                                                     .SingleOrDefault();

            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) settledFee = aggregate.SettledCalculatedFees.Where(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId)
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

            MerchantFeeSettledEvent merchantFeeSettledEvent = new MerchantFeeSettledEvent(aggregate.AggregateId,
                                                                                          aggregate.EstateId,
                                                                                          pendingFee.merchantId,
                                                                                          pendingFee.transactionId,
                                                                                          pendingFee.calculatedFee.CalculatedValue,
                                                                                          (Int32)pendingFee.calculatedFee.FeeCalculationType,
                                                                                          pendingFee.calculatedFee.FeeId,
                                                                                          pendingFee.calculatedFee.FeeValue,
                                                                                          pendingFee.calculatedFee.FeeCalculatedDateTime);

            aggregate.ApplyAndAppend(merchantFeeSettledEvent);
        }

        public static void AddFee(this SettlementAggregate aggregate,
                           Guid merchantId,
                           Guid transactionId,
                           CalculatedFee calculatedFee)
        {
            Guard.ThrowIfInvalidGuid(merchantId, nameof(merchantId));
            Guard.ThrowIfInvalidGuid(transactionId, nameof(merchantId));
            Guard.ThrowIfNull(calculatedFee, nameof(calculatedFee));

            aggregate.CheckHasBeenCreated();
            if (aggregate.HasFeeAlreadyBeenAdded(transactionId, calculatedFee))
                return;

            DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.Merchant)
            {
                // This is a merchant fee
                @event = new MerchantFeeAddedPendingSettlementEvent(aggregate.AggregateId,
                                                                    aggregate.EstateId,
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

            aggregate.ApplyAndAppend(@event);
        }

        public static List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> GetFeesToBeSettled(this SettlementAggregate aggregate)
        {
            return aggregate.CalculatedFeesPendingSettlement;
        }

        public static void Create(this SettlementAggregate aggregate, Guid estateId,
                           DateTime settlementDate)
        {
            aggregate.CheckHasNotAlreadyBeenCreated();

            SettlementCreatedForDateEvent pendingSettlementCreatedForDateEvent =
                new SettlementCreatedForDateEvent(aggregate.AggregateId, estateId, settlementDate.Date);

            aggregate.ApplyAndAppend(pendingSettlementCreatedForDateEvent);
        }

        public static Int32 GetNumberOfFeesPendingSettlement(this SettlementAggregate aggregate)
        {
            return aggregate.CalculatedFeesPendingSettlement.Count;
        }

        public static Int32 GetNumberOfFeesSettled(this SettlementAggregate aggregate)
        {
            return aggregate.SettledCalculatedFees.Count;
        }

        private static Boolean HasFeeAlreadyBeenAdded(this SettlementAggregate aggregate, Guid transactionId, CalculatedFee calculatedFee)
        {
            return aggregate.CalculatedFeesPendingSettlement.Any(c => c.calculatedFee.FeeId == calculatedFee.FeeId && c.transactionId == transactionId) ||
                   aggregate.SettledCalculatedFees.Any(c => c.calculatedFee.FeeId == calculatedFee.FeeId && c.transactionId == transactionId);
        }

        private static void CheckHasBeenCreated(this SettlementAggregate aggregate)
        {
            if (aggregate.IsCreated == false)
            {
                throw new InvalidOperationException($"Pending Settlement not created for this date {aggregate.SettlementDate}");
            }
        }

        private static void CheckHasNotAlreadyBeenCreated(this SettlementAggregate aggregate)
        {
            if (aggregate.IsCreated)
            {
                throw new InvalidOperationException($"Pending Settlement already created for this date {aggregate.SettlementDate}");
            }
        }

        public static void PlayEvent(this SettlementAggregate aggregate, MerchantFeeSettledEvent domainEvent)
        {
            // Add to the settled fees list
            aggregate.SettledCalculatedFees.Add(new(domainEvent.TransactionId,
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
            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) feeToRemove = aggregate.CalculatedFeesPendingSettlement
                                                                                                      .Single(f => f.transactionId == domainEvent.TransactionId &&
                                                                                                                   f.merchantId == domainEvent.MerchantId && f.calculatedFee.FeeId == domainEvent.FeeId);

            aggregate.CalculatedFeesPendingSettlement.Remove(feeToRemove);
        }

        public static void PlayEvent(this SettlementAggregate aggregate, MerchantFeeAddedPendingSettlementEvent domainEvent)
        {
            aggregate.CalculatedFeesPendingSettlement.Add(new(domainEvent.TransactionId,
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

        public static void PlayEvent(this SettlementAggregate aggregate, SettlementCreatedForDateEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.SettlementDate = domainEvent.SettlementDate;
            aggregate.IsCreated = true;
        }

        public static void PlayEvent(this SettlementAggregate aggregate, SettlementCompletedEvent domainEvent)
        {
            aggregate.SettlementComplete = true;
        }
    }

    public record SettlementAggregate : Aggregate
    {
        #region Fields

        /// <summary>
        /// The calculated fees
        /// </summary>
        internal readonly List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> CalculatedFeesPendingSettlement;

        internal readonly List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> SettledCalculatedFees;

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

        public Guid EstateId { get; internal set; }

        public Boolean IsCreated { get; internal set; }

        public DateTime SettlementDate { get; internal set; }

        public Boolean SettlementComplete { get; internal set; }

        #endregion

        #region Methods
        
        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public static SettlementAggregate Create(Guid aggregateId)
        {
            return new SettlementAggregate(aggregateId);
        }

        public override void PlayEvent(IDomainEvent domainEvent) => SettlementAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }
        
        #endregion
    }
}