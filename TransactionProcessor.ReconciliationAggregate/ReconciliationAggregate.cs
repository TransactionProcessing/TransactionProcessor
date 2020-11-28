using System;

namespace TransactionProcessor.ReconciliationAggregate
{
    using System.Diagnostics.CodeAnalysis;
    using Reconciliation.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.EventStore;
    using Shared.General;
    using Transaction.DomainEvents;

    public class ReconciliationAggregate : Aggregate
    {
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
                   {
                       this.EstateId
                   };
        }

        protected override void PlayEvent(DomainEvent domainEvent)
        {
            this.PlayEvent((dynamic)domainEvent);
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconciliationAggregate" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ReconciliationAggregate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconciliationAggregate" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private ReconciliationAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
        }

        public Guid EstateId { get; private set; }
        public Boolean IsStarted { get; private set; }
        public Guid MerchantId { get; private set; }
        public String ResponseCode { get; private set; }
        public Boolean IsCompleted { get; private set; }
        public Boolean IsAuthorised { get; private set; }
        public Boolean IsDeclined { get; private set; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; private set; }

        public Int32 TransactionCount { get; private set; }

        public Decimal TransactionValue { get; private set; }

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public static ReconciliationAggregate Create(Guid aggregateId)
        {
            return new ReconciliationAggregate(aggregateId);
        }

        private void CheckReconciliationNotAlreadyStarted()
        {
            if (this.IsStarted)
            {
                throw new InvalidOperationException($"Reconciliation Id [{this.AggregateId}] has already been started");
            }
        }

        private void CheckReconciliationNotAlreadyCompleted()
        {
            if (this.IsCompleted)
            {
                throw new InvalidOperationException($"Reconciliation Id [{this.AggregateId}] has already been completed");
            }
        }

        public void StartReconciliation(DateTime transactionDateTime,
                                        Guid estateId,
                                        Guid merchantId)
        {
            Guard.ThrowIfInvalidGuid(estateId, typeof(ArgumentException), $"Estate Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidGuid(merchantId, typeof(ArgumentException), $"Merchant Id must not be [{Guid.Empty}]");
            Guard.ThrowIfInvalidDate(transactionDateTime, typeof(ArgumentException), $"Transaction Date Time must not be [{DateTime.MinValue}]");

            // TODO: Some rules here
            this.CheckReconciliationNotAlreadyStarted();
            this.CheckReconciliationNotAlreadyCompleted();

            ReconciliationHasStartedEvent reconciliationHasStartedEvent =
                ReconciliationHasStartedEvent.Create(this.AggregateId, estateId, merchantId, transactionDateTime);

            this.ApplyAndPend(reconciliationHasStartedEvent);
        }

        private void CheckReconciliationHasBeenStarted()
        {
            if (this.IsStarted == false)
            {
                throw new InvalidOperationException($"Reconciliation [{this.AggregateId}] has not been started");
            }
        }

        public void RecordOverallTotals(Int32 totalCount, Decimal totalValue)
        {
            // TODO: Rules
            this.CheckReconciliationHasBeenStarted();
            this.CheckReconciliationNotAlreadyCompleted();

            OverallTotalsRecordedEvent overallTotalsRecordedEvent = OverallTotalsRecordedEvent.Create(this.AggregateId, this.EstateId,this.MerchantId, totalCount, totalValue);
            this.ApplyAndPend(overallTotalsRecordedEvent);
        }

        public void Authorise(String responseCode, String responseMessage)
        {
            this.CheckReconciliationHasBeenStarted();
            this.CheckReconciliationNotAlreadyCompleted();
            this.CheckReconciliationNotAlreadyAuthorised();
            this.CheckReconciliationNotAlreadyDeclined();

            ReconciliationHasBeenLocallyAuthorisedEvent reconciliationHasBeenLocallyAuthorisedEvent = ReconciliationHasBeenLocallyAuthorisedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId,
                 responseCode, responseMessage);

            this.ApplyAndPend(reconciliationHasBeenLocallyAuthorisedEvent);
        }

        private void CheckReconciliationNotAlreadyAuthorised()
        {
            if (this.IsAuthorised)
            {
                throw new InvalidOperationException($"Reconciliation [{this.AggregateId}] has already been authorised");
            }
        }

        private void CheckReconciliationNotAlreadyDeclined()
        {
            if (this.IsDeclined)
            {
                throw new InvalidOperationException($"Reconciliation [{this.AggregateId}] has already been declined");
            }
        }

        private void CheckReconciliationHasBeenAuthorisedOrDeclined()
        {
            if (this.IsAuthorised == false && this.IsDeclined == false)
            {
                throw new InvalidOperationException($"Reconciliation [{this.AggregateId}] has not been authorised or declined");
            }
        }

        public void Decline(String responseCode, String responseMessage)
        {
            this.CheckReconciliationHasBeenStarted();
            this.CheckReconciliationNotAlreadyCompleted();
            this.CheckReconciliationNotAlreadyAuthorised();
            this.CheckReconciliationNotAlreadyDeclined();

            ReconciliationHasBeenLocallyDeclinedEvent reconciliationHasBeenLocallyDeclinedEvent = ReconciliationHasBeenLocallyDeclinedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId,
                responseCode, responseMessage);

            this.ApplyAndPend(reconciliationHasBeenLocallyDeclinedEvent);
        }

        public void CompleteReconciliation()
        {
            this.CheckReconciliationHasBeenStarted();
            this.CheckReconciliationNotAlreadyCompleted();
            this.CheckReconciliationHasBeenAuthorisedOrDeclined();

            ReconciliationHasCompletedEvent reconciliationHasCompletedEvent =
                ReconciliationHasCompletedEvent.Create(this.AggregateId, this.EstateId, this.MerchantId);

            this.ApplyAndPend(reconciliationHasCompletedEvent);

        }
        #endregion

        private void PlayEvent(ReconciliationHasStartedEvent domainEvent)
        {
            this.EstateId = domainEvent.EstateId;
            this.MerchantId = domainEvent.MerchantId;
            this.IsStarted = true;
        }

        private void PlayEvent(OverallTotalsRecordedEvent domainEvent)
        {
            this.TransactionCount = domainEvent.TransactionCount;
            this.TransactionValue = domainEvent.TransactionValue;
        }

        private void PlayEvent(ReconciliationHasBeenLocallyAuthorisedEvent domainEvent)
        {
            this.ResponseCode = domainEvent.ResponseCode;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.IsAuthorised = true;
        }

        private void PlayEvent(ReconciliationHasBeenLocallyDeclinedEvent domainEvent)
        {
            this.ResponseCode = domainEvent.ResponseCode;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.IsDeclined = true;
        }

        private void PlayEvent(ReconciliationHasCompletedEvent domainEvent)
        {
            this.IsStarted = false;
            this.IsCompleted = true;
        }
    }
}
