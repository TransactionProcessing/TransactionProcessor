using System;

namespace TransactionProcessor.ReconciliationAggregate
{
    using System.Diagnostics.CodeAnalysis;
    using Reconciliation.DomainEvents;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.General;
    using Transaction.DomainEvents;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.EventStore.Aggregate.Aggregate" />
    public class ReconciliationAggregate : Aggregate
    {
        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return new
                   {
                       this.EstateId
                   };
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        public override void PlayEvent(IDomainEvent domainEvent)
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

        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is started; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsStarted { get; private set; }
        /// <summary>
        /// Gets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; private set; }
        /// <summary>
        /// Gets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public String ResponseCode { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is completed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is completed; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsCompleted { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is authorised.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is authorised; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsAuthorised { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this instance is declined.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is declined; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsDeclined { get; private set; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; private set; }

        /// <summary>
        /// Gets the transaction count.
        /// </summary>
        /// <value>
        /// The transaction count.
        /// </value>
        public Int32 TransactionCount { get; private set; }

        /// <summary>
        /// Gets the transaction value.
        /// </summary>
        /// <value>
        /// The transaction value.
        /// </value>
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

        /// <summary>
        /// Checks the reconciliation not already started.
        /// </summary>
        /// <exception cref="InvalidOperationException">Reconciliation Id [{this.AggregateId}] has already been started</exception>
        private void CheckReconciliationNotAlreadyStarted()
        {
            if (this.IsStarted)
            {
                throw new InvalidOperationException($"Reconciliation Id [{this.AggregateId}] has already been started");
            }
        }

        /// <summary>
        /// Checks the reconciliation not already completed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Reconciliation Id [{this.AggregateId}] has already been completed</exception>
        private void CheckReconciliationNotAlreadyCompleted()
        {
            if (this.IsCompleted)
            {
                throw new InvalidOperationException($"Reconciliation Id [{this.AggregateId}] has already been completed");
            }
        }

        /// <summary>
        /// Starts the reconciliation.
        /// </summary>
        /// <param name="transactionDateTime">The transaction date time.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
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
                new ReconciliationHasStartedEvent(this.AggregateId, estateId, merchantId, transactionDateTime);

            this.ApplyAndAppend(reconciliationHasStartedEvent);
        }

        /// <summary>
        /// Checks the reconciliation has been started.
        /// </summary>
        /// <exception cref="InvalidOperationException">Reconciliation [{this.AggregateId}] has not been started</exception>
        private void CheckReconciliationHasBeenStarted()
        {
            if (this.IsStarted == false)
            {
                throw new InvalidOperationException($"Reconciliation [{this.AggregateId}] has not been started");
            }
        }

        /// <summary>
        /// Records the overall totals.
        /// </summary>
        /// <param name="totalCount">The total count.</param>
        /// <param name="totalValue">The total value.</param>
        public void RecordOverallTotals(Int32 totalCount, Decimal totalValue)
        {
            // TODO: Rules
            this.CheckReconciliationHasBeenStarted();
            this.CheckReconciliationNotAlreadyCompleted();

            OverallTotalsRecordedEvent overallTotalsRecordedEvent = new OverallTotalsRecordedEvent(this.AggregateId, this.EstateId,this.MerchantId, totalCount, totalValue);
            this.ApplyAndAppend(overallTotalsRecordedEvent);
        }

        /// <summary>
        /// Authorises the specified response code.
        /// </summary>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public void Authorise(String responseCode, String responseMessage)
        {
            this.CheckReconciliationHasBeenStarted();
            this.CheckReconciliationNotAlreadyCompleted();
            this.CheckReconciliationNotAlreadyAuthorised();
            this.CheckReconciliationNotAlreadyDeclined();

            ReconciliationHasBeenLocallyAuthorisedEvent reconciliationHasBeenLocallyAuthorisedEvent = new ReconciliationHasBeenLocallyAuthorisedEvent(this.AggregateId, this.EstateId, this.MerchantId,
                 responseCode, responseMessage);

            this.ApplyAndAppend(reconciliationHasBeenLocallyAuthorisedEvent);
        }

        /// <summary>
        /// Checks the reconciliation not already authorised.
        /// </summary>
        /// <exception cref="InvalidOperationException">Reconciliation [{this.AggregateId}] has already been authorised</exception>
        private void CheckReconciliationNotAlreadyAuthorised()
        {
            if (this.IsAuthorised)
            {
                throw new InvalidOperationException($"Reconciliation [{this.AggregateId}] has already been authorised");
            }
        }

        /// <summary>
        /// Checks the reconciliation not already declined.
        /// </summary>
        /// <exception cref="InvalidOperationException">Reconciliation [{this.AggregateId}] has already been declined</exception>
        private void CheckReconciliationNotAlreadyDeclined()
        {
            if (this.IsDeclined)
            {
                throw new InvalidOperationException($"Reconciliation [{this.AggregateId}] has already been declined");
            }
        }

        /// <summary>
        /// Checks the reconciliation has been authorised or declined.
        /// </summary>
        /// <exception cref="InvalidOperationException">Reconciliation [{this.AggregateId}] has not been authorised or declined</exception>
        private void CheckReconciliationHasBeenAuthorisedOrDeclined()
        {
            if (this.IsAuthorised == false && this.IsDeclined == false)
            {
                throw new InvalidOperationException($"Reconciliation [{this.AggregateId}] has not been authorised or declined");
            }
        }

        /// <summary>
        /// Declines the specified response code.
        /// </summary>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public void Decline(String responseCode, String responseMessage)
        {
            this.CheckReconciliationHasBeenStarted();
            this.CheckReconciliationNotAlreadyCompleted();
            this.CheckReconciliationNotAlreadyAuthorised();
            this.CheckReconciliationNotAlreadyDeclined();

            ReconciliationHasBeenLocallyDeclinedEvent reconciliationHasBeenLocallyDeclinedEvent = new ReconciliationHasBeenLocallyDeclinedEvent(this.AggregateId, this.EstateId, this.MerchantId,
                responseCode, responseMessage);

            this.ApplyAndAppend(reconciliationHasBeenLocallyDeclinedEvent);
        }

        /// <summary>
        /// Completes the reconciliation.
        /// </summary>
        public void CompleteReconciliation()
        {
            this.CheckReconciliationHasBeenStarted();
            this.CheckReconciliationNotAlreadyCompleted();
            this.CheckReconciliationHasBeenAuthorisedOrDeclined();

            ReconciliationHasCompletedEvent reconciliationHasCompletedEvent =
                new ReconciliationHasCompletedEvent(this.AggregateId, this.EstateId, this.MerchantId);

            this.ApplyAndAppend(reconciliationHasCompletedEvent);

        }
        #endregion

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(ReconciliationHasStartedEvent domainEvent)
        {
            this.EstateId = domainEvent.EstateId;
            this.MerchantId = domainEvent.MerchantId;
            this.IsStarted = true;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(OverallTotalsRecordedEvent domainEvent)
        {
            this.TransactionCount = domainEvent.TransactionCount;
            this.TransactionValue = domainEvent.TransactionValue;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(ReconciliationHasBeenLocallyAuthorisedEvent domainEvent)
        {
            this.ResponseCode = domainEvent.ResponseCode;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.IsAuthorised = true;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(ReconciliationHasBeenLocallyDeclinedEvent domainEvent)
        {
            this.ResponseCode = domainEvent.ResponseCode;
            this.ResponseMessage = domainEvent.ResponseMessage;
            this.IsDeclined = true;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(ReconciliationHasCompletedEvent domainEvent)
        {
            this.IsStarted = false;
            this.IsCompleted = true;
        }
    }
}
