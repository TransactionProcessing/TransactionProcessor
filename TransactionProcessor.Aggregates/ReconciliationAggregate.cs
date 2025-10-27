using SimpleResults;
using TransactionProcessor.DomainEvents;

namespace TransactionProcessor.Aggregates
{
    using System.Diagnostics.CodeAnalysis;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;

    public static class ReconciliationAggregateExtensions{
        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationDomainEvents.ReconciliationHasStartedEvent domainEvent)
        {
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;
            aggregate.HasBeenStarted = true;
            aggregate.TransactionDateTime = domainEvent.TransactionDateTime;
        }

        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationDomainEvents.OverallTotalsRecordedEvent domainEvent)
        {
            aggregate.TransactionCount = domainEvent.TransactionCount;
            aggregate.TransactionValue = domainEvent.TransactionValue;
        }

        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationDomainEvents.ReconciliationHasBeenLocallyAuthorisedEvent domainEvent)
        {
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
            aggregate.IsAuthorised = true;
        }

        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationDomainEvents.ReconciliationHasBeenLocallyDeclinedEvent domainEvent)
        {
            aggregate.ResponseCode = domainEvent.ResponseCode;
            aggregate.ResponseMessage = domainEvent.ResponseMessage;
            aggregate.IsDeclined = true;
        }

        public static void PlayEvent(this ReconciliationAggregate aggregate, ReconciliationDomainEvents.ReconciliationHasCompletedEvent domainEvent)
        {
            aggregate.IsCompleted = true;
        }

        private static Result CheckReconciliationNotAlreadyCompleted(this ReconciliationAggregate aggregate) {
            if (aggregate.IsCompleted) {
                return Result.Invalid($"Reconciliation Id [{aggregate.AggregateId}] has already been completed");
            }
            return Result.Success();
        }

        public static Result StartReconciliation(this ReconciliationAggregate aggregate,
                                                 DateTime transactionDateTime,
                                                 Guid estateId,
                                                 Guid merchantId) {
            if (estateId == Guid.Empty)
                return Result.Invalid($"Estate Id must not be [{Guid.Empty}]");
            if (merchantId == Guid.Empty)
                return Result.Invalid($"Merchant Id must not be [{Guid.Empty}]");
            if (transactionDateTime == DateTime.MinValue)
                return Result.Invalid($"Transaction Date Time must not be [{DateTime.MinValue}]");

            if (aggregate.HasBeenStarted || aggregate.IsCompleted) {
                return Result.Success();
            }

            ReconciliationDomainEvents.ReconciliationHasStartedEvent reconciliationHasStartedEvent = new(aggregate.AggregateId, estateId, merchantId, transactionDateTime);

            aggregate.ApplyAndAppend(reconciliationHasStartedEvent);

            return Result.Success();
        }

        private static Result CheckReconciliationHasBeenStarted(this ReconciliationAggregate aggregate) {
            if (aggregate.HasBeenStarted == false) {
                return Result.Invalid($"Reconciliation [{aggregate.AggregateId}] has not been started");
            }
            return Result.Success();
        }

        public static Result RecordOverallTotals(this ReconciliationAggregate aggregate, Int32 totalCount, Decimal totalValue)
        {
            Result result = aggregate.CheckReconciliationHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckReconciliationNotAlreadyCompleted();
            if (result.IsFailed)
                return result;

            ReconciliationDomainEvents.OverallTotalsRecordedEvent overallTotalsRecordedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, totalCount, totalValue, aggregate.TransactionDateTime);
            aggregate.ApplyAndAppend(overallTotalsRecordedEvent);

            return Result.Success();
        }

        public static Result Authorise(this ReconciliationAggregate aggregate, TransactionResponseCode responseCode, String responseMessage)
        {
            if (aggregate.IsAuthorised || aggregate.IsDeclined) {
                return Result.Success();
            }

            Result result = aggregate.CheckReconciliationHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckReconciliationNotAlreadyCompleted();
            if (result.IsFailed)
                return result;

            ReconciliationDomainEvents.ReconciliationHasBeenLocallyAuthorisedEvent reconciliationHasBeenLocallyAuthorisedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId,
                                                                                                                                                      responseCode.ToCodeString(), responseMessage, aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(reconciliationHasBeenLocallyAuthorisedEvent);
            return Result.Success();
        }
        
        private static Result CheckReconciliationHasBeenAuthorisedOrDeclined(this ReconciliationAggregate aggregate)
        {
            if (aggregate.IsAuthorised == false && aggregate.IsDeclined == false)
            {
                return Result.Invalid($"Reconciliation [{aggregate.AggregateId}] has not been authorised or declined");
            }
            return Result.Success();
        }

        public static Result Decline(this ReconciliationAggregate aggregate,TransactionResponseCode responseCode, String responseMessage)
        {
            if (aggregate.IsAuthorised || aggregate.IsDeclined) {
                return Result.Success();
            }

            Result result = aggregate.CheckReconciliationHasBeenStarted();
            if (result.IsFailed)
                return result;
            result = aggregate.CheckReconciliationNotAlreadyCompleted();
            if (result.IsFailed)
                return result;

            ReconciliationDomainEvents.ReconciliationHasBeenLocallyDeclinedEvent reconciliationHasBeenLocallyDeclinedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId,
                                                                                                                                                responseCode.ToCodeString(), responseMessage, aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(reconciliationHasBeenLocallyDeclinedEvent);

            return Result.Success();
        }

        public static Result CompleteReconciliation(this ReconciliationAggregate aggregate)
        {
            if (aggregate.IsCompleted)
                return Result.Success();

            Result result = aggregate.CheckReconciliationHasBeenAuthorisedOrDeclined();
            if (result.IsFailed)
                return result;

            ReconciliationDomainEvents.ReconciliationHasCompletedEvent reconciliationHasCompletedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, aggregate.TransactionDateTime);

            aggregate.ApplyAndAppend(reconciliationHasCompletedEvent);

            return Result.Success();
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
            if (aggregateId == Guid.Empty)
                throw new ArgumentNullException(nameof(aggregateId));

            this.AggregateId = aggregateId;
        }

        public Guid EstateId { get; internal set; }

        public Boolean HasBeenStarted { get; internal set; }
        
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
