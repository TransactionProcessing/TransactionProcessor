using System;

namespace TransactionProcessor.Aggregates
{
    using System.Diagnostics.CodeAnalysis;
    using Reconciliation.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.General;

    public static class ReconciliationAggregateExtensions{
        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationHasStartedEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;
            aggregate.IsStarted = true;
            aggregate.TransactionDateTime = domainEvent.TransactionDateTime;
        }

        public static void PlayEvent(this ReconciliationAggregate aggregate, OverallTotalsRecordedEvent domainEvent)
        {
            aggregate.TransactionCount = domainEvent.TransactionCount;
            aggregate.TransactionValue = domainEvent.TransactionValue;
        }

        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationHasBeenLocallyAuthorisedEvent domainEvent)
        {
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
            aggregate.IsAuthorised = true;
        }

        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationHasBeenLocallyDeclinedEvent domainEvent)
        {
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
            aggregate.IsDeclined = true;
        }

        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationHasCompletedEvent domainEvent)
        {
            aggregate.IsStarted = false;
            aggregate.IsCompleted = true;
        }

        private static void CheckReconciliationNotAlreadyStarted(this ReconciliationAggregate aggregate)
        {
            if (aggregate.IsStarted)
            {
                throw new InvalidOperationException($"Reconciliation Id [{aggregate.AggregateId}] has already been started");
            }
        }

        private static void CheckReconciliationNotAlreadyCompleted(this ReconciliationAggregate aggregate)
        {
            if (aggregate.IsCompleted)
            {
                throw new InvalidOperationException($"Reconciliation Id [{aggregate.AggregateId}] has already been completed");
            }
        }

        public static void StartReconciliation(this ReconciliationAggregate aggregate, DateTime transactionDateTime,
                                               Guid estateId,
                                               Guid merchantId)
        {
            Guard.ThrowIfInvalidGuid(estateId, typeof(ArgumentException), $"Estate Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(merchantId, typeof(ArgumentException), $"Merchant Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidDate(transactionDateTime, typeof(ArgumentException), $"Transaction Date Time must not be [{DateTime.MinValue}]");

            // TODO: Some rules here
            aggregate.CheckReconciliationNotAlreadyStarted();
            aggregate.CheckReconciliationNotAlreadyCompleted();

            ReconciliationHasStartedEvent reconciliationHasStartedEvent =
                new ReconciliationHasStartedEvent(aggregate.AggregateId, estateId, merchantId, transactionDateTime);

            aggregate.ApplyAndAppend(reconciliationHasStartedEvent);
        }

        private static void CheckReconciliationHasBeenStarted(this ReconciliationAggregate aggregate)
        {
            if (aggregate.IsStarted == false)
            {
                throw new InvalidOperationException($"Reconciliation [{aggregate.AggregateId}] has not been started");
            }
        }

        public static void RecordOverallTotals(this ReconciliationAggregate aggregate, Int32 totalCount, Decimal totalValue)
        {
            // TODO: Rules
            aggregate.CheckReconciliationHasBeenStarted();
            aggregate.CheckReconciliationNotAlreadyCompleted();

            OverallTotalsRecordedEvent overallTotalsRecordedEvent = new OverallTotalsRecordedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, totalCount, totalValue, aggregate.TransactionDateTime);
            aggregate.ApplyAndAppend(overallTotalsRecordedEvent);
        }

        public static void Authorise(this ReconciliationAggregate aggregate, String responseCode, String responseMessage)
        {
            aggregate.CheckReconciliationHasBeenStarted();
            aggregate.CheckReconciliationNotAlreadyCompleted();
            aggregate.CheckReconciliationNotAlreadyAuthorised();
            aggregate.CheckReconciliationNotAlreadyDeclined();

            ReconciliationHasBeenLocallyAuthorisedEvent reconciliationHasBeenLocallyAuthorisedEvent = new ReconciliationHasBeenLocallyAuthorisedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId,
                                                                                                                                                      responseCode, responseMessage, aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(reconciliationHasBeenLocallyAuthorisedEvent);
        }

        private static void CheckReconciliationNotAlreadyAuthorised(this ReconciliationAggregate aggregate)
        {
            if (aggregate.IsAuthorised)
            {
                throw new InvalidOperationException($"Reconciliation [{aggregate.AggregateId}] has already been authorised");
            }
        }

        private static void CheckReconciliationNotAlreadyDeclined(this ReconciliationAggregate aggregate)
        {
            if (aggregate.IsDeclined)
            {
                throw new InvalidOperationException($"Reconciliation [{aggregate.AggregateId}] has already been declined");
            }
        }

        private static void CheckReconciliationHasBeenAuthorisedOrDeclined(this ReconciliationAggregate aggregate)
        {
            if (aggregate.IsAuthorised == false && aggregate.IsDeclined == false)
            {
                throw new InvalidOperationException($"Reconciliation [{aggregate.AggregateId}] has not been authorised or declined");
            }
        }

        public static void Decline(this ReconciliationAggregate aggregate,String responseCode, String responseMessage)
        {
            aggregate.CheckReconciliationHasBeenStarted();
            aggregate.CheckReconciliationNotAlreadyCompleted();
            aggregate.CheckReconciliationNotAlreadyAuthorised();
            aggregate.CheckReconciliationNotAlreadyDeclined();

            ReconciliationHasBeenLocallyDeclinedEvent reconciliationHasBeenLocallyDeclinedEvent = new ReconciliationHasBeenLocallyDeclinedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId,
                                                                                                                                                responseCode, responseMessage, aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(reconciliationHasBeenLocallyDeclinedEvent);
        }

        public static void CompleteReconciliation(this ReconciliationAggregate aggregate)
        {
            aggregate.CheckReconciliationHasBeenStarted();
            aggregate.CheckReconciliationNotAlreadyCompleted();
            aggregate.CheckReconciliationHasBeenAuthorisedOrDeclined();

            ReconciliationHasCompletedEvent reconciliationHasCompletedEvent =
                new ReconciliationHasCompletedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(reconciliationHasCompletedEvent);

        }

    }

    public record ReconciliationAggregate : Aggregate
    {
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
                   {
                       this.EstateId
                   };
        }


        public override void PlayEvent(IDomainEvent domainEvent) => ReconciliationAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        #region Constructors

        [ExcludeFromCodeCoverage]
        public ReconciliationAggregate()
        {
        }

        private ReconciliationAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
        }

        public Guid EstateId { get; internal set; }

        public Boolean IsStarted { get; internal set; }
        
        public Guid MerchantId { get; internal set; }
        
        public String ResponseCode { get; internal set; }
        
        public Boolean IsCompleted { get; internal set; }
        
        public Boolean IsAuthorised { get; internal set; }
        
        public Boolean IsDeclined { get; internal set; }

        public String ResponseMessage { get; internal set; }

        public Int32 TransactionCount { get; internal set; }

        public Decimal TransactionValue { get; internal set; }

        public DateTime TransactionDateTime { get; internal set; }

        public static ReconciliationAggregate Create(Guid aggregateId)
        {
            return new ReconciliationAggregate(aggregateId);
        }
        
        #endregion

    }
}
