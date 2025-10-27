using SimpleResults;
using TransactionProcessor.DomainEvents;
using TransactionProcessor.Models;
using TransactionProcessor.Models.Contract;

namespace TransactionProcessor.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;

    public static class SettlementAggregateExtensions{
        public static Result StartProcessing(this SettlementAggregate aggregate, DateTime dateTime){

            Result result = aggregate.CheckHasBeenCreated();
            if (result.IsFailed)
                return result;
            if (aggregate.ProcessingStarted)
                return Result.Success();

            SettlementDomainEvents.SettlementProcessingStartedEvent startedEvent = new SettlementDomainEvents.SettlementProcessingStartedEvent(aggregate.AggregateId,
                                                                                                 aggregate.EstateId,
                                                                                                 aggregate.MerchantId,
                                                                                                 dateTime);
            aggregate.ApplyAndAppend(startedEvent);

            return Result.Success();
        }

        public static Result ManuallyComplete(this SettlementAggregate aggregate){

            if (aggregate.SettlementComplete)
                return Result.Success();

            Result result = aggregate.CheckHasBeenCreated();
            if (result.IsFailed)
                return result;

            SettlementDomainEvents.SettlementCompletedEvent pendingSettlementCompletedEvent = new SettlementDomainEvents.SettlementCompletedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId);
            aggregate.ApplyAndAppend(pendingSettlementCompletedEvent);

            return Result.Success();
        }

        public static Result MarkFeeAsSettled(this SettlementAggregate aggregate, Guid merchantId, Guid transactionId, Guid feeId, DateTime settledDate)
        {
            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) pendingFee = GetPendingFee(aggregate, merchantId, transactionId, feeId);

            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) settledFee = GetSettledFee(aggregate, merchantId, transactionId, feeId);

            if (settledFee != default((Guid, Guid, CalculatedFee))) {
                // Fee already settled....
                return Result.Success();
            }

            if (pendingFee == default((Guid, Guid, CalculatedFee))) {
                // Fee not found....
                return Result.Success();
            }

            SettlementDomainEvents.MerchantFeeSettledEvent merchantFeeSettledEvent = CreateMerchantFeeSettledEvent(aggregate, pendingFee,settledDate);

            aggregate.ApplyAndAppend(merchantFeeSettledEvent);

            if (aggregate.CalculatedFeesPendingSettlement.Any() == false)
            {
                // Settlement is completed
                SettlementDomainEvents.SettlementCompletedEvent pendingSettlementCompletedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId);
                aggregate.ApplyAndAppend(pendingSettlementCompletedEvent);
            }
            return Result.Success();
        }

        private static (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) GetSettledFee(SettlementAggregate aggregate, Guid merchantId, Guid transactionId, Guid feeId){
            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) settledFee = aggregate.SettledCalculatedFees
                                                                                                     .SingleOrDefault(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId);
            return settledFee;
        }

        private static SettlementDomainEvents.MerchantFeeSettledEvent CreateMerchantFeeSettledEvent(SettlementAggregate aggregate, (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) feeDetails,
                                                                                                    DateTime settledDate){
            SettlementDomainEvents.MerchantFeeSettledEvent merchantFeeSettledEvent = new SettlementDomainEvents.MerchantFeeSettledEvent(aggregate.AggregateId,
                                                                                          aggregate.EstateId,
                                                                                          feeDetails.merchantId,
                                                                                          feeDetails.transactionId,
                                                                                          feeDetails.calculatedFee.CalculatedValue,
                                                                                          (Int32)feeDetails.calculatedFee.FeeCalculationType,
                                                                                          feeDetails.calculatedFee.FeeId,
                                                                                          feeDetails.calculatedFee.FeeValue,
                                                                                          feeDetails.calculatedFee.FeeCalculatedDateTime,
                                                                                          settledDate);
            return merchantFeeSettledEvent;
        }

        public static Result ImmediatelyMarkFeeAsSettled(this SettlementAggregate aggregate, Guid merchantId, Guid transactionId, Guid feeId)
        {
            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) pendingFee = SettlementAggregateExtensions.GetPendingFee(aggregate, merchantId, transactionId, feeId);

            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) settledFee = SettlementAggregateExtensions.GetSettledFee(aggregate, merchantId, transactionId, feeId);

            if (settledFee != default((Guid, Guid, CalculatedFee))) {
                // Fee already settled....
                return Result.Success();
            }

            if (pendingFee == default((Guid, Guid, CalculatedFee))) {
                // Fee not found....
                return Result.Success();
            }

            SettlementDomainEvents.MerchantFeeSettledEvent merchantFeeSettledEvent = CreateMerchantFeeSettledEvent(aggregate, pendingFee,DateTime.Now);

            aggregate.ApplyAndAppend(merchantFeeSettledEvent);
            
            return Result.Success();
        }

        private static (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) GetPendingFee(SettlementAggregate aggregate, Guid merchantId, Guid transactionId, Guid feeId){
            (Guid transactionId, Guid merchantId, CalculatedFee calculatedFee) pendingFee = aggregate.CalculatedFeesPendingSettlement
                                                                                                     .SingleOrDefault(c => c.merchantId == merchantId && c.transactionId == transactionId && c.calculatedFee.FeeId == feeId);
            return pendingFee;
        }

        public static Result AddFee(this SettlementAggregate aggregate,
                                  Guid merchantId,
                                  Guid transactionId,
                                  CalculatedFee calculatedFee)
        {
            if (merchantId == Guid.Empty)
                return Result.Invalid("Merchant Id cannot be an empty Guid");
            if (transactionId == Guid.Empty) {
                return Result.Invalid("Merchant Id cannot be an empty Guid");
            }
            if (calculatedFee == null)
                return Result.Invalid("Calculated Fee cannot be null");

            Result result = aggregate.CheckHasBeenCreated();
            if (result.IsFailed)
                return result;

            if (aggregate.HasFeeAlreadyBeenAdded(transactionId, calculatedFee))
                return Result.Success();

            DomainEvent @event = null;
            if (calculatedFee.FeeType == FeeType.Merchant)
            {
                // This is a merchant fee
                @event = new SettlementDomainEvents.MerchantFeeAddedPendingSettlementEvent(aggregate.AggregateId,
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
                return Result.Invalid("Unsupported Fee Type");
            }

            aggregate.ApplyAndAppend(@event);

            return Result.Success();
        }

        public static List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)> GetFeesToBeSettled(this SettlementAggregate aggregate)
        {
            return aggregate.CalculatedFeesPendingSettlement;
        }

        public static Result Create(this SettlementAggregate aggregate, Guid estateId,Guid merchantId,
                           DateTime settlementDate)
        {
            if (aggregate.IsCreated)
                return Result.Success();

            SettlementDomainEvents.SettlementCreatedForDateEvent pendingSettlementCreatedForDateEvent = new(aggregate.AggregateId, estateId, merchantId, settlementDate.Date);

            aggregate.ApplyAndAppend(pendingSettlementCreatedForDateEvent);

            return Result.Success();
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

        private static Result CheckHasBeenCreated(this SettlementAggregate aggregate)
        {
            if (aggregate.IsCreated == false) {
                return Result.Invalid($"Pending Settlement not created for this date {aggregate.SettlementDate}");
            }
            return Result.Success();
        }
        
        public static void PlayEvent(this SettlementAggregate aggregate, SettlementDomainEvents.MerchantFeeSettledEvent domainEvent)
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

        public static void PlayEvent(this SettlementAggregate aggregate, SettlementDomainEvents.MerchantFeeAddedPendingSettlementEvent domainEvent)
        {
            aggregate.CalculatedFeesPendingSettlement.Add(new(domainEvent.TransactionId,
                                                              domainEvent.MerchantId,
                                                              new CalculatedFee
                                                              {
                                                                  CalculatedValue = domainEvent.CalculatedValue,
                                                                  FeeId = domainEvent.FeeId,
                                                                  FeeType = FeeType.Merchant,
                                                                  FeeValue = domainEvent.FeeValue,
                                                                  FeeCalculationType = (CalculationType)domainEvent.FeeCalculationType,
                                                                  FeeCalculatedDateTime = domainEvent.FeeCalculatedDateTime
                                                              }));
        }

        public static void PlayEvent(this SettlementAggregate aggregate, SettlementDomainEvents.SettlementCreatedForDateEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;
            aggregate.SettlementDate = domainEvent.SettlementDate;
            aggregate.IsCreated = true;
        }

        public static void PlayEvent(this SettlementAggregate aggregate, SettlementDomainEvents.SettlementCompletedEvent domainEvent)
        {
            aggregate.SettlementComplete = true;
        }

        public static void PlayEvent(this SettlementAggregate aggregate, SettlementDomainEvents.SettlementProcessingStartedEvent domainEvent){
            aggregate.ProcessingStarted= true;
            aggregate.ProcessingStartedDateTime = domainEvent.ProcessingStartedDateTime;
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
            if (aggregateId == Guid.Empty)
                throw new ArgumentNullException(nameof(aggregateId));

            this.AggregateId = aggregateId;
            this.CalculatedFeesPendingSettlement = new List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)>();
            this.SettledCalculatedFees = new List<(Guid transactionId, Guid merchantId, CalculatedFee calculatedFee)>();
        }

        #endregion

        #region Properties

        public Guid EstateId { get; internal set; }

        public Guid MerchantId { get; internal set; }

        public Boolean IsCreated { get; internal set; }

        public DateTime SettlementDate { get; internal set; }

        public Boolean SettlementComplete { get; internal set; }

        public Boolean ProcessingStarted { get; internal set; }

        public DateTime ProcessingStartedDateTime { get; internal set; }
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